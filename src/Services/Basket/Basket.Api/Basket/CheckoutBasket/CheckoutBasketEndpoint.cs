namespace Basket.Api.Basket.CheckoutBasket;

public record CheckoutBasketRequest(Checkout Checkout);

public record CheckoutBasketResponse(bool IsSuccess);

public class CheckoutBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, IMediator mediator) =>
        {
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
}
