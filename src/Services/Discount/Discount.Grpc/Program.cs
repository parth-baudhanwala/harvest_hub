using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string discountConnection = builder.Configuration.GetConnectionString("Discount")!;

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContextPool<DiscountContext>(options =>
{
    options.UseSqlite(discountConnection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
