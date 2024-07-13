using Identity.Api.Data.Context;
using Identity.Api.Data.Seeds;

var builder = WebApplication.CreateBuilder(args);

string assembly = typeof(Program).Assembly.FullName!;
string identityConnection = builder.Configuration.GetConnectionString("Identity")!;

builder.Services.AddDbContext<AspNetIdentityDbContext>(options =>
{
    options.UseSqlServer(identityConnection, opt => opt.MigrationsAssembly(assembly));
});

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AspNetIdentityDbContext>();

builder.Services
    .AddIdentityServer()
    .AddAspNetIdentity<IdentityUser>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = config =>
        config.UseSqlServer(identityConnection, opt => opt.MigrationsAssembly(assembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = config =>
        config.UseSqlServer(identityConnection, opt => opt.MigrationsAssembly(assembly));
    })
    .AddDeveloperSigningCredential();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

app.UseIdentityServer();

await app.RunAsync();
