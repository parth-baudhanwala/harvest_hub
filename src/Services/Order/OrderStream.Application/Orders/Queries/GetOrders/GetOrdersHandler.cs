
namespace OrderStream.Application.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        int index = query.PaginationRequest.Index;
        int size = query.PaginationRequest.Size;

        long count = await dbContext.Orders.LongCountAsync(cancellationToken);

        var orders = await dbContext.Orders.Include(x => x.Items)
                                           .AsNoTracking()
                                           .Take(size)
                                           .Skip(index * size)
                                           .OrderBy(x => x.Name.Value)
                                           .ToListAsync(cancellationToken);

        return new(new PaginatedResult<OrderDto>(index, size, count, orders.ToOrderDtoList()));
    }
}
