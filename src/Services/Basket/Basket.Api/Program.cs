using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.MessageBroker.MassTransit;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
string basketDbConnection = builder.Configuration.GetConnectionString("Basket")!;
string redisConnection = builder.Configuration.GetConnectionString("Redis")!;
string discountUrl = builder.Configuration["GrpcSettings:DiscountUrl"]!;
string authority = builder.Configuration["Authority"]!;

#region Services

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Read", config => config.RequireClaim("scope", "basket_read"))
    .AddPolicy("Write", config => config.RequireClaim("scope", "basket_write"));

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
    HttpClientHandler handler = new()
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
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

app.UseHttpsRedirection();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();
