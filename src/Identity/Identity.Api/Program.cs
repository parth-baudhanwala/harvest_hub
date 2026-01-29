using BuildingBlocks.MessageBroker.Events;
using BuildingBlocks.MessageBroker.MassTransit;
using Identity.Api.Data.Context;
using Identity.Api.Data.Seeds;
using Identity.Api.Models;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("spa", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

string assembly = typeof(Program).Assembly.FullName!;
string identityConnection = builder.Configuration.GetConnectionString("Identity")!;
JwtSettings jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services.AddDbContext<AspNetIdentityDbContext>(options =>
{
    options.UseSqlServer(identityConnection, opt => opt.MigrationsAssembly(assembly));
});

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AspNetIdentityDbContext>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token)
                    && context.Request.Cookies.TryGetValue("hh_admin_access_token", out var adminToken))
                {
                    context.Token = adminToken;
                }

                if (string.IsNullOrWhiteSpace(context.Token)
                    && context.Request.Cookies.TryGetValue("hh_access_token", out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddMessageBroker(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

app.UseCors("spa");

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<IdentityUser> userManager, IPublishEndpoint publishEndpoint) =>
{
    var existing = await userManager.FindByEmailAsync(request.Email);
    if (existing is not null)
    {
        return Results.BadRequest(new { message = "Email already registered." });
    }

    var user = new IdentityUser
    {
        UserName = request.Username,
        Email = request.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { message = result.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserRegisteredEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email
    });

    return Results.Ok(new { message = "User registered successfully." });
});

app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<IdentityUser> userManager, HttpContext httpContext) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var scopes = new List<string>
    {
        "catalog_read",
        "basket_read",
        "basket_write",
        "order_read",
        "order_write"
    };

    if (roles.Contains("Admin"))
    {
        scopes.Add("catalog_write");
    }

    var scopeClaims = scopes.Select(scope => new Claim("scope", scope));

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new(ClaimTypes.Name, user.UserName ?? string.Empty)
    };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    claims.AddRange(scopeClaims);

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(jwtSettings.TokenLifetimeMinutes);

    var token = new JwtSecurityToken(
        issuer: jwtSettings.Issuer,
        audience: jwtSettings.Audience,
        claims: claims,
        expires: expires,
        signingCredentials: creds
    );

    var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Expires = expires,
        Path = "/"
    };

    httpContext.Response.Cookies.Append("hh_access_token", tokenValue, cookieOptions);

    return Results.Ok(new
    {
        user = new { user.Id, user.UserName, user.Email }
    });
});

app.MapPost("/api/admin/login", async (LoginRequest request, UserManager<IdentityUser> userManager, HttpContext httpContext) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    if (!roles.Contains("Admin"))
    {
        return Results.Forbid();
    }

    var scopes = new List<string>
    {
        "catalog_read",
        "basket_read",
        "basket_write",
        "order_read",
        "order_write",
        "catalog_write"
    };

    var scopeClaims = scopes.Select(scope => new Claim("scope", scope));

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new(ClaimTypes.Name, user.UserName ?? string.Empty)
    };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    claims.AddRange(scopeClaims);

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(jwtSettings.TokenLifetimeMinutes);

    var token = new JwtSecurityToken(
        issuer: jwtSettings.Issuer,
        audience: jwtSettings.Audience,
        claims: claims,
        expires: expires,
        signingCredentials: creds
    );

    var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Expires = expires,
        Path = "/"
    };

    httpContext.Response.Cookies.Append("hh_admin_access_token", tokenValue, cookieOptions);

    return Results.Ok(new
    {
        user = new { user.Id, user.UserName, user.Email }
    });
});

app.MapPost("/api/auth/logout", (HttpContext httpContext) =>
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };

    httpContext.Response.Cookies.Delete("hh_access_token", cookieOptions);
    return Results.Ok();
});

app.MapPost("/api/admin/logout", (HttpContext httpContext) =>
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = httpContext.Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };

    httpContext.Response.Cookies.Delete("hh_admin_access_token", cookieOptions);
    return Results.Ok();
});

app.MapGet("/api/auth/me", [Authorize] (ClaimsPrincipal principal) =>
{
    var roles = principal.FindAll(ClaimTypes.Role).Select(role => role.Value).ToArray();

    return Results.Ok(new
    {
        id = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
        email = principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email),
        userName = principal.FindFirstValue(ClaimTypes.Name),
        roles
    });
});

app.MapGet("/api/admin/users", [Authorize(Roles = "Admin")] async (UserManager<IdentityUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    var results = new List<object>();

    foreach (var user in users)
    {
        var roles = await userManager.GetRolesAsync(user);
        results.Add(new
        {
            id = user.Id,
            userName = user.UserName,
            email = user.Email,
            roles
        });
    }

    return Results.Ok(results);
});

app.MapGet("/api/admin/users/{id}", [Authorize(Roles = "Admin")] async (string id, UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    var roles = await userManager.GetRolesAsync(user);

    return Results.Ok(new
    {
        id = user.Id,
        userName = user.UserName,
        email = user.Email,
        roles
    });
});

app.MapPut("/api/admin/users/{id}", [Authorize(Roles = "Admin")] async (
    string id,
    UpdateUserRequest request,
    UserManager<IdentityUser> userManager,
    IPublishEndpoint publishEndpoint) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null && existing.Id != user.Id)
        {
            return Results.BadRequest(new { message = "Email already registered." });
        }
    }

    user.UserName = request.Username;
    user.Email = request.Email;

    var updateResult = await userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
        return Results.BadRequest(new { message = updateResult.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserUpdatedEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email
    });

    return Results.Ok(new { user.Id, user.UserName, user.Email });
});

app.MapDelete("/api/admin/users/{id}", [Authorize(Roles = "Admin")] async (
    string id,
    UserManager<IdentityUser> userManager,
    IPublishEndpoint publishEndpoint) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    var result = await userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { message = result.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserDeletedEvent
    {
        UserId = user.Id,
        Email = user.Email
    });

    return Results.NoContent();
});

app.MapGet("/api/admin/admins", [Authorize(Roles = "Admin")] async (UserManager<IdentityUser> userManager) =>
{
    var admins = await userManager.GetUsersInRoleAsync("Admin");
    var results = new List<object>();

    foreach (var admin in admins)
    {
        var roles = await userManager.GetRolesAsync(admin);
        results.Add(new
        {
            id = admin.Id,
            userName = admin.UserName,
            email = admin.Email,
            roles
        });
    }

    return Results.Ok(results);
});

app.MapPost("/api/admin/admins", [Authorize(Roles = "Admin")] async (
    AdminUserCreateRequest request,
    UserManager<IdentityUser> userManager,
    IPublishEndpoint publishEndpoint) =>
{
    var existing = await userManager.FindByEmailAsync(request.Email);
    if (existing is not null)
    {
        return Results.BadRequest(new { message = "Email already registered." });
    }

    var user = new IdentityUser
    {
        UserName = request.Username,
        Email = request.Email,
        EmailConfirmed = true
    };

    var createResult = await userManager.CreateAsync(user, request.Password);
    if (!createResult.Succeeded)
    {
        return Results.BadRequest(new { message = createResult.Errors.First().Description });
    }

    var addRoleResult = await userManager.AddToRoleAsync(user, "Admin");
    if (!addRoleResult.Succeeded)
    {
        return Results.BadRequest(new { message = addRoleResult.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserRegisteredEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email
    });

    await publishEndpoint.Publish(new AdminUserUpsertedEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email,
        IsAdmin = true
    });

    return Results.Ok(new { user.Id, user.UserName, user.Email });
});

app.MapPut("/api/admin/admins/{id}", [Authorize(Roles = "Admin")] async (
    string id,
    UpdateUserRequest request,
    UserManager<IdentityUser> userManager,
    IPublishEndpoint publishEndpoint) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    if (!await userManager.IsInRoleAsync(user, "Admin"))
    {
        return Results.BadRequest(new { message = "User is not an admin." });
    }

    if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null && existing.Id != user.Id)
        {
            return Results.BadRequest(new { message = "Email already registered." });
        }
    }

    user.UserName = request.Username;
    user.Email = request.Email;

    var updateResult = await userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
        return Results.BadRequest(new { message = updateResult.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserUpdatedEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email
    });

    await publishEndpoint.Publish(new AdminUserUpsertedEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? request.Username,
        Email = user.Email ?? request.Email,
        IsAdmin = true
    });

    return Results.Ok(new { user.Id, user.UserName, user.Email });
});

app.MapDelete("/api/admin/admins/{id}", [Authorize(Roles = "Admin")] async (
    string id,
    UserManager<IdentityUser> userManager,
    IPublishEndpoint publishEndpoint) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }

    if (!await userManager.IsInRoleAsync(user, "Admin"))
    {
        return Results.BadRequest(new { message = "User is not an admin." });
    }

    var result = await userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { message = result.Errors.First().Description });
    }

    await publishEndpoint.Publish(new UserDeletedEvent
    {
        UserId = user.Id,
        Email = user.Email
    });

    await publishEndpoint.Publish(new AdminUserUpsertedEvent
    {
        UserId = user.Id,
        Username = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        IsAdmin = false
    });

    return Results.NoContent();
});

await app.RunAsync();
