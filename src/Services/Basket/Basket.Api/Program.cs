using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.MessageBroker.MassTransit;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
string basketDbConnection = builder.Configuration.GetConnectionString("Basket")!;
string redisConnection = builder.Configuration.GetConnectionString("Redis")!;
string discountUrl = builder.Configuration["GrpcSettings:DiscountUrl"]!;
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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Read", config => config.RequireClaim("scope", "basket_read"))
    .AddPolicy("Write", config => config.RequireClaim("scope", "basket_write"))
    .AddPolicy("Admin", config => config.RequireRole("Admin"));

builder.Services.AddCarter();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviors<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddMarten(config =>
{
    config.Connection(basketDbConnection);
    config.Schema.For<ShoppingCart>().Identity(x => x.Username);
}).UseLightweightSessions();

builder.Services.AddStackExchangeRedisCache(setup =>
{
    setup.Configuration = redisConnection;
    //setup.InstanceName = "Basket";
});

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(config =>
{
    config.Address = new Uri(discountUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    if (builder.Environment.IsDevelopment())
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    return new HttpClientHandler();
});

builder.Services.AddMessageBroker(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, BasketCacheRepository>();

// Manually Decorate
//builder.Services.AddScoped(provider =>
//{
//    var basketRepository = provider.GetRequiredService<BasketRepository>();
//    var distributedCache = provider.GetRequiredService<IDistributedCache>();
//    return new BasketCacheRepository(basketRepository, distributedCache);
//});

builder.Services.AddHealthChecks()
                .AddNpgSql(basketDbConnection)
                .AddRedis(redisConnection);

#endregion

var app = builder.Build();

// Run Services

app.UseAuthentication();

app.UseAuthorization();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).RequireAuthorization("Admin");

await app.RunAsync();
