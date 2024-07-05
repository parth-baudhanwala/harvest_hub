namespace OrderStream.Application.Orders.Queries.GetOrdersByName;

public class GetOrdersByNameQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    public async Task<GetOrdersByNameResult> Handle(GetOrdersByNameQuery query, CancellationToken cancellationToken)
    {
        OrderName orderName = OrderName.Of(query.Name);

        var orders = await dbContext.Orders.Include(x => x.Items)
                                           .AsNoTracking()
                                           .Where(x => x.Name == orderName)
                                           .OrderBy(x => x.Name.Value)
                                           .ToListAsync(cancellationToken);

        return new(orders.ToOrderDtoList());
    }
}
