using Identity.Api.Data.Context;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Security.Claims;

namespace Identity.Api.Data.Seeds;

public static class DatabaseSeeds
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();

        await persistedGrantContext.Database.MigrateAsync();

        var configurationContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        await configurationContext.Database.MigrateAsync();

        await SeedAsync(configurationContext);

        var aspNetIdentityContext = scope.ServiceProvider.GetRequiredService<AspNetIdentityDbContext>();

        await aspNetIdentityContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedUserAsync(userManager);
    }

    private static async Task SeedAsync(ConfigurationDbContext configurationContext)
    {
        if (!await configurationContext.Clients.AnyAsync())
        {
            foreach (var client in InitialData.Clients.ToList())
            {
                configurationContext.Clients.Add(client.ToEntity());
            }

            await configurationContext.SaveChangesAsync();
        }

        if (!await configurationContext.IdentityResources.AnyAsync())
        {
            foreach (var resource in InitialData.IdentityResources.ToList())
            {
                configurationContext.IdentityResources.Add(resource.ToEntity());
            }

            await configurationContext.SaveChangesAsync();
        }

        if (!await configurationContext.ApiScopes.AnyAsync())
        {
            foreach (var resource in InitialData.ApiScopes.ToList())
            {
                configurationContext.ApiScopes.Add(resource.ToEntity());
            }

            await configurationContext.SaveChangesAsync();
        }

        if (!await configurationContext.ApiResources.AnyAsync())
        {
            foreach (var resource in InitialData.ApiResources.ToList())
            {
                configurationContext.ApiResources.Add(resource.ToEntity());
            }

            await configurationContext.SaveChangesAsync();
        }
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager)
    {
        IdentityUser? pbaudhanwala = await userManager.FindByNameAsync("pbaudhanwala");

        if (pbaudhanwala is not null)
        {
            return;
        }

        pbaudhanwala = new()
        {
            UserName = "pbaudhanwala",
            Email = "parthbaudhanwala45@gmail.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(pbaudhanwala, "Pass123$");

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.Errors.First().Description);
        }

        IEnumerable<Claim> claims =
        [
            new Claim(JwtClaimTypes.Name, "Parth Baudhanwala"),
            new Claim(JwtClaimTypes.GivenName, "Parth"),
            new Claim(JwtClaimTypes.FamilyName, "Baudhanwala"),
            new Claim(JwtClaimTypes.Email, "parthbaudhanwala45@gmail.com"),
            new Claim(JwtClaimTypes.WebSite, "https://parthbaudhanwalaofficial.com"),
            new Claim("location", "somewhere")
        ];

        result = await userManager.AddClaimsAsync(pbaudhanwala, claims);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.Errors.First().Description);
        }
    }

    private static async Task RemoveAllAsync(ConfigurationDbContext configurationContext)
    {
        var clients = await configurationContext.Clients.ToListAsync();
        configurationContext.Clients.RemoveRange(clients);

        var identityResources = await configurationContext.IdentityResources.ToListAsync();
        configurationContext.IdentityResources.RemoveRange(identityResources);

        var apiScopes = await configurationContext.ApiScopes.ToListAsync();
        configurationContext.ApiScopes.RemoveRange(apiScopes);

        var apiResources = await configurationContext.ApiResources.ToListAsync();
        configurationContext.ApiResources.RemoveRange(apiResources);

        await configurationContext.SaveChangesAsync();
    }
}
