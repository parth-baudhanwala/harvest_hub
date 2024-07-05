using MassTransit;
using Microsoft.FeatureManagement;

namespace OrderStream.Application.Orders.EventHandlers.Domain;

public class OrderUpdatedEventHandler(IPublishEndpoint publishEndpoint,
                                      IFeatureManager featureManager,
                                      ILogger<OrderUpdatedEventHandler> logger)
    : INotificationHandler<OrderUpdatedEvent>
{
    public async Task Handle(OrderUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handled: {DomainEvent}", domainEvent.GetType().Name);

        if (await featureManager.IsEnabledAsync("OrderFullfilment"))
        {
            OrderDto orderUpdatedIntegrationEvent = domainEvent.Order.ToOrderDto();
            await publishEndpoint.Publish(orderUpdatedIntegrationEvent, cancellationToken);
        }
    }
}
