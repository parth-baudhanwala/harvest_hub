using BuildingBlocks.Pagination;

namespace Catalog.Api.Products.GetProducts;

public record GetProductsResponse(PaginatedResult<Product> Products);

public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([AsParameters] PaginationRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductsQuery(request));
            var response = result.Adapt<GetProductsResponse>();
            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .WithSummary("Get Products")
        .WithDescription("Get Products")
        .Produces<GetProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
