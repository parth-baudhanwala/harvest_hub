using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Basket.Api.Basket.CheckoutBasket;

public record CheckoutBasketRequest(Checkout Checkout);

public record CheckoutBasketResponse(bool IsSuccess);

public class CheckoutBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsOwnerOrAdmin(user, request.Checkout.Username))
            {
                return Results.Forbid();
            }

            if (!IsCustomerMatchOrAdmin(user, request.Checkout.CustomerId))
            {
                return Results.Forbid();
            }

            var command = request.Adapt<CheckoutBasketCommand>();
            var result = await mediator.Send(command);
            var response = result.Adapt<CheckoutBasketResponse>();

            return Results.Ok(response);
        })
        .WithName("CheckoutBasket")
        .Produces<CheckoutBasketResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Checkout Basket")
        .WithDescription("Checkout Basket")
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

    private static bool IsCustomerMatchOrAdmin(ClaimsPrincipal user, Guid customerId)
    {
        if (user.IsInRole("Admin"))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return !string.IsNullOrWhiteSpace(userId)
               && customerId != Guid.Empty
               && string.Equals(userId, customerId.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
