using Identity.Api.Data.Context;
using Identity.Api.Data.Seeds;
using Identity.Api.Models;
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
                    && context.Request.Cookies.TryGetValue("hh_access_token", out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

app.UseCors("spa");

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<IdentityUser> userManager) =>
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
    var scopeClaims = new[]
    {
        "catalog_read",
        "catalog_write",
        "basket_read",
        "basket_write",
        "order_read",
        "order_write"
    }.Select(scope => new Claim("scope", scope));

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

app.MapGet("/api/auth/me", [Authorize] (ClaimsPrincipal principal) =>
{
    return Results.Ok(new
    {
        id = principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
        email = principal.FindFirstValue(JwtRegisteredClaimNames.Email),
        userName = principal.FindFirstValue(ClaimTypes.Name)
    });
});

await app.RunAsync();
