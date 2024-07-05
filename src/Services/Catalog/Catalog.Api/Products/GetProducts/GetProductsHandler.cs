using BuildingBlocks.Pagination;
using Marten.Pagination;

namespace Catalog.Api.Products.GetProducts;

public record GetProductsQuery(PaginationRequest PaginationRequest) : IQuery<GetProductsResult>;

public record GetProductsResult(PaginatedResult<Product> Products);

internal class GetProductsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        int index = query.PaginationRequest.Index;
        int size = query.PaginationRequest.Size;

        long count = await session.Query<Product>().LongCountAsync(cancellationToken);

        var products = await session.Query<Product>()
                                    .ToPagedListAsync(query.PaginationRequest.Index, query.PaginationRequest.Size, cancellationToken);

        return new(new PaginatedResult<Product>(index, size, count, products));
    }
}
