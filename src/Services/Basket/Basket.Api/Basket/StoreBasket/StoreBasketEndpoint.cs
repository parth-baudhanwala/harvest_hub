namespace Basket.Api.Basket.StoreBasket;

public record StoreBasketRequest(ShoppingCart Cart);

public record StoreBasketResponse(bool IsSuccess);

public class StoreBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (StoreBasketRequest request, IMediator mediator) =>
        {
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
}
