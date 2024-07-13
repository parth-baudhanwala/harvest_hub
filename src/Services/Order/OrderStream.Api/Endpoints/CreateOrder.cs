﻿using OrderStream.Application.Orders.Commands.CreateOrder;

namespace OrderStream.Api.Endpoints;

public record CreateOrderRequest(OrderDto Order);

public record CreateOrderResponse(Guid Id);

public class CreateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest request, IMediator mediator) =>
        {
            var command = request.Adapt<CreateOrderCommand>();
            var result = await mediator.Send(command);
            var response = result.Adapt<CreateOrderResponse>();

            return Results.Created($"/orders/{response.Id}", response);
        })
        .WithName("CreateOrder")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Order")
        .WithDescription("Create Order")
        .RequireAuthorization("Write");
    }
}
