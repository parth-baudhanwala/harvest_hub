namespace Catalog.Api.Products.UpdateProduct;

public record UpdateProductRequest(Guid Id,
                                   string Name,
                                   List<string> Categories,
                                   string Description,
                                   string ImageFile,
                                   decimal Price);

public record UpdateProductResponse(bool IsSuccess);

public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products", async (UpdateProductRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateProductCommand(request.Id, request.Name, request.Categories, request.Description, request.ImageFile, request.Price));
            var response = result.Adapt<UpdateProductResponse>();
            return Results.Ok(response);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update Product")
        .WithDescription("Update Product")
        .Produces<UpdateProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization("Admin");
    }
}
