namespace Basket.Api.Basket.GetBasket;

public record GetBasketResponse(ShoppingCart Cart);

public class GetBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{username}", async (string username, IMediator mediator) =>
        {
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
}
