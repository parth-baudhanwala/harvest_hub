using BuildingBlocks.MessageBroker.Events;
using MassTransit;
using OrderStream.Application.Orders.Commands.CreateOrder;
using OrderStream.Domain.Enums;

namespace OrderStream.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler(IMediator mediator, ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var command = MapToCreateOrderCommand(context.Message);
        await mediator.Send(command);
    }

    private static CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
    {
        AddressDto addressDto = new(message.FirstName, message.LastName, message.EmailAddress, message.AddressLine, message.Country, message.State, message.ZipCode);
        PaymentDto paymentDto = new(message.CardName, message.CardNumber, message.Expiration, message.Cvv, message.PaymentMethod);

        Guid orderId = Guid.NewGuid();

        OrderDto orderDto = new(
            Id: orderId,
            CustomerId: message.CustomerId,
            Name: message.Username,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            Status: OrderStatus.Pending,
            Items: message.Items.Select(x => new OrderItemDto(orderId, x.ProductId, x.Quantity, x.Price)).ToList()
        );

        return new(orderDto);
    }
}
