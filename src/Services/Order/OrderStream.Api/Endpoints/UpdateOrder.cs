using OrderStream.Application.Orders.Commands.UpdateOrder;

namespace OrderStream.Api.Endpoints;

public record UpdateOrderRequest(OrderDto Order);

public record UpdateOrderResponse(Guid Id);

public class UpdateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/orders", async (UpdateOrderRequest request, IMediator mediator) =>
        {
            var command = request.Adapt<UpdateOrderCommand>();
            var result = await mediator.Send(command);
            var response = result.Adapt<UpdateOrderResponse>();

            return Results.Ok(response);
        })
        .WithName("UpdateOrder")
        .Produces<UpdateOrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Order")
        .WithDescription("Update Order");
    }
}
