namespace Catalog.Api.Products.CreateProduct;

public record CreateProductRequest(string Name,
                                   List<string> Categories,
                                   string Description,
                                   string ImageFileName,
                                   byte[] ImageBytes,
                                   string? ImageContentType,
                                   decimal Price);

public record CreateProductResponse(Guid Id);

public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest request, IMediator mediator) =>
        {
            CreateProductCommand command = request.Adapt<CreateProductCommand>();
            CreateProductResult result = await mediator.Send(command);
            CreateProductResponse response = result.Adapt<CreateProductResponse>();
            return Results.Created($"/products/{response.Id}", response);
        })
        .WithName("CreateProduct")
        .WithSummary("Create Product")
        .WithDescription("Create Product")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization("Write");
    }
}
