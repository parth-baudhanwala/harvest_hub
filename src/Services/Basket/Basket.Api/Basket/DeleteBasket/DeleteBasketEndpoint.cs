using Basket.Api.Basket.GetBasket;
using System.Security.Claims;

namespace Basket.Api.Basket.DeleteBasket;

public record DeleteBasketResponse(bool IsSuccess);

public class DeleteBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{username}", async (string username, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsOwnerOrAdmin(user, username))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new DeleteBasketCommand(username));
            var response = result.Adapt<DeleteBasketResponse>();
            return Results.Ok(response);
        })
        .WithName("DeleteBasket")
        .WithSummary("Delete Basket")
        .WithDescription("Delete Basket")
        .Produces<GetBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
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
