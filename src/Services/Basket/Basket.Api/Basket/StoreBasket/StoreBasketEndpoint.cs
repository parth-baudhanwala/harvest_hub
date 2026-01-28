using System.Security.Claims;

namespace Basket.Api.Basket.StoreBasket;

public record StoreBasketRequest(ShoppingCart Cart);

public record StoreBasketResponse(bool IsSuccess);

public class StoreBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (StoreBasketRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsOwnerOrAdmin(user, request.Cart.Username))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new StoreBasketCommand(request.Cart));
            var response = result.Adapt<StoreBasketResponse>();
            return Results.Created($"/basket/{response.IsSuccess}", response);
        })
        .WithName("StoreBasket")
        .WithSummary("Store Basket")
        .WithDescription("Store Basket")
        .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization("Write");
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
