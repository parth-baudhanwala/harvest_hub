using Identity.Api.Data.Context;
using System.Security.Claims;

namespace Identity.Api.Data.Seeds;

public static class DatabaseSeeds
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var aspNetIdentityContext = scope.ServiceProvider.GetRequiredService<AspNetIdentityDbContext>();

        await aspNetIdentityContext.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedUserAsync(userManager);
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
            new Claim(ClaimTypes.Name, "Parth Baudhanwala"),
            new Claim(ClaimTypes.GivenName, "Parth"),
            new Claim(ClaimTypes.Surname, "Baudhanwala"),
            new Claim(ClaimTypes.Email, "parthbaudhanwala45@gmail.com"),
            new Claim("website", "https://parthbaudhanwalaofficial.com"),
            new Claim("location", "somewhere")
        ];

        result = await userManager.AddClaimsAsync(pbaudhanwala, claims);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(result.Errors.First().Description);
        }
    }

}
