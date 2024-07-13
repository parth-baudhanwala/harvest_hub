namespace Catalog.Api.Products.DeleteProduct;

public record DeleteProductRequest(Guid Id);

public record DeleteProductResponse(bool IsSuccess);

public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductCommand(id));
            var response = result.Adapt<DeleteProductResponse>();
            return response;
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete Product")
        .WithDescription("Delete Product")
        .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization("Write");
    }
}
