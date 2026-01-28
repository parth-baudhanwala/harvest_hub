using OrderStream.Application.Orders.Commands.CreateOrder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderStream.Api.Endpoints;

public record CreateOrderRequest(OrderDto Order);

public record CreateOrderResponse(Guid Id);

public class CreateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsCustomerMatchOrAdmin(user, request.Order.CustomerId))
            {
                return Results.Forbid();
            }

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
