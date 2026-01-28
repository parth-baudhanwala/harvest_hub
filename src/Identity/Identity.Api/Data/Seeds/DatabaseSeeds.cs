using BuildingBlocks.MessageBroker.Events;
using Identity.Api.Data.Context;
using MassTransit;
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
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await SeedUserAsync(userManager, publishEndpoint);
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, IPublishEndpoint publishEndpoint)
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

        await publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = pbaudhanwala.Id,
            Username = pbaudhanwala.UserName ?? string.Empty,
            Email = pbaudhanwala.Email ?? string.Empty
        });

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
