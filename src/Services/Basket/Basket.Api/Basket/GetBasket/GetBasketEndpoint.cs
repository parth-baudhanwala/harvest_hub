using System.Security.Claims;

namespace Basket.Api.Basket.GetBasket;

public record GetBasketResponse(ShoppingCart Cart);

public class GetBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{username}", async (string username, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsOwnerOrAdmin(user, username))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new GetBasketQuery(username));
            var response = result.Adapt<GetBasketResponse>();
            return Results.Ok(response);
        })
        .WithName("GetBasket")
        .WithSummary("Get Basket")
        .WithDescription("Get Basket")
        .Produces<GetBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization("Read");
    }

    private static bool IsOwnerOrAdmin(ClaimsPrincipal user, string username)
    {
        if (user.IsInRole("Admin"))
        {
            return true;
        }

        var name = user.FindFirstValue(ClaimTypes.Name) ?? user.Identity?.Name;
        return !string.IsNullOrWhiteSpace(name)
               && string.Equals(name, username, StringComparison.OrdinalIgnoreCase);
    }
}
