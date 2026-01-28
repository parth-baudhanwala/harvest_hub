using OrderStream.Application.Orders.Queries.GetOrdersByCustomer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderStream.Api.Endpoints;

public record GetOrdersByCustomerResponse(IEnumerable<OrderDto> Orders);

public class GetOrdersByCustomer : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/customer/{customerId}", async (Guid customerId, IMediator mediator, ClaimsPrincipal user) =>
        {
            if (!IsCustomerMatchOrAdmin(user, customerId))
            {
                return Results.Forbid();
            }

            var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
            var response = result.Adapt<GetOrdersByCustomerResponse>();

            return Results.Ok(response);
        })
        .WithName("GetOrdersByCustomer")
        .Produces<GetOrdersByCustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Orders By Customer")
        .WithDescription("Get Orders By Customer")
        .RequireAuthorization("Read");
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
