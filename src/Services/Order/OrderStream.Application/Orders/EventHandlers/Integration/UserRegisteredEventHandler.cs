using BuildingBlocks.MessageBroker.Events;
using MassTransit;

namespace OrderStream.Application.Orders.EventHandlers.Integration;

public class UserRegisteredEventHandler(IApplicationDbContext dbContext, ILogger<UserRegisteredEventHandler> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        if (!Guid.TryParse(context.Message.UserId, out var userId) || userId == Guid.Empty)
        {
            logger.LogWarning("UserRegisteredEvent contains invalid UserId: {UserId}", context.Message.UserId);
            return;
        }

        var customerId = CustomerId.Of(userId);
        var exists = await dbContext.Customers
            .AnyAsync(c => c.Id == customerId, context.CancellationToken);

        if (exists) return;

        var name = string.IsNullOrWhiteSpace(context.Message.Username)
            ? context.Message.Email
            : context.Message.Username;

        var customer = Customer.Create(customerId, name, context.Message.Email);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
