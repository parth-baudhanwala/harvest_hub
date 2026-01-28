using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.MessageBroker.MassTransit;
using Catalog.Api.Data;
using Catalog.Api.Storage;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
string catalogDbConnection = builder.Configuration.GetConnectionString("Catalog")!;
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
string issuer = jwtSettings["Issuer"]!;
string audience = jwtSettings["Audience"]!;
string signingKey = jwtSettings["SigningKey"]!;

#region Services

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Read", config => config.RequireClaim("scope", "catalog_read"))
    .AddPolicy("Write", config => config.RequireClaim("scope", "catalog_write"));

builder.Services.AddCarter();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviors<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMarten(config =>
{
    config.Connection(catalogDbConnection);
}).UseLightweightSessions();

builder.Services.AddHttpClient();

builder.Services.Configure<MinioStorageOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MinioStorageOptions>>().Value;
    return new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .WithSSL(options.UseSsl)
        .Build();
});
builder.Services.AddSingleton<IProductImageStorage, MinioProductImageStorage>();

if (builder.Environment.IsDevelopment()) builder.Services.InitializeMartenWith<CatalogInitialData>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddMessageBroker(builder.Configuration);

builder.Services.AddHealthChecks().AddNpgSql(catalogDbConnection);

#endregion

var app = builder.Build();

// Run Services

app.UseAuthentication();

app.UseAuthorization();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

#region hidden local exception handler
//app.UseExceptionHandler(config =>
//{
//    config.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

//        if (exception is null) return;

//        ProblemDetails problemDetails = new()
//        {
//            Title = exception.Message,
//            Status = StatusCodes.Status500InternalServerError,
//            Detail = exception.StackTrace
//        };

//        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//        logger.LogError(exception, "{@Message}", exception.Message);

//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        context.Response.ContentType = "application/problem+json";

//        await context.Response.WriteAsJsonAsync(problemDetails);
//    });
//});
#endregion

await app.RunAsync();
