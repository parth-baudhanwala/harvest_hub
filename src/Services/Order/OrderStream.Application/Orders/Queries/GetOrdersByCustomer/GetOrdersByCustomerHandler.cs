namespace OrderStream.Application.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        CustomerId customerId = CustomerId.Of(query.CustomerId);

        var orders = await dbContext.Orders.Include(x => x.Items)
                                           .AsNoTracking()
                                           .Where(x => x.CustomerId == customerId)
                                           .OrderBy(x => x.Name.Value)
                                           .ToListAsync(cancellationToken);

        return new(orders.ToOrderDtoList());
    }
}
