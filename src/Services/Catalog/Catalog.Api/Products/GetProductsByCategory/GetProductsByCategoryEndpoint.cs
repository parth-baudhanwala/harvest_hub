namespace Catalog.Api.Products.GetProductsByCategory;

public record GetProductsByCategoryResponse(IEnumerable<Product> Products);

public class GetProductsByCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", async (string category, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductsByCategoryQuery(category));
            var response = result.Adapt<GetProductsByCategoryResponse>();
            return Results.Ok(response);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get Products By Category")
        .WithDescription("Get Products By Category")
        .Produces<GetProductsByCategoryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
