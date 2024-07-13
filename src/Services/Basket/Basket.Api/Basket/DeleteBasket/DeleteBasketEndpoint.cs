using Basket.Api.Basket.GetBasket;

namespace Basket.Api.Basket.DeleteBasket;

public record DeleteBasketResponse(bool IsSuccess);

public class DeleteBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{username}", async (string username, IMediator mediator) =>
        {
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
}
