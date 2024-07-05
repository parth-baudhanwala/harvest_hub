namespace OrderStream.Application.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<DeleteOrderCommand, DeleteOrderResult>
{
    public async Task<DeleteOrderResult> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        OrderId orderId = OrderId.Of(command.OrderId);

        Order order = await dbContext.Orders.FindAsync([orderId], cancellationToken)
                            ?? throw new OrderNotFoundException(command.OrderId);

        dbContext.Orders.Remove(order);

        bool isSuccess = await dbContext.SaveChangesAsync(cancellationToken) > 0;

        return new(isSuccess);
    }
}
