using BuildingBlocks.MessageBroker.Events;
using MassTransit;

namespace Basket.Api.Basket.CheckoutBasket;

public record CheckoutBasketCommand(Checkout Checkout) : ICommand<CheckoutBasketResult>;

public record CheckoutBasketResult(bool IsSuccess);

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.Checkout).NotNull().WithMessage("Checkout can't be null.");
        RuleFor(x => x.Checkout.Username).NotEmpty().WithMessage("Username is required.");
    }
}

public class CheckoutBasketCommandHandler(IBasketRepository repository, IPublishEndpoint publishEndpoint) : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.Checkout.Username, cancellationToken);

        if (basket is null) return new(false);

        BasketCheckoutEvent eventMessage = CreateEventMessage(command.Checkout, basket);

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        await repository.DeleteBasket(command.Checkout.Username, cancellationToken);

        return new(true);
    }

    private static BasketCheckoutEvent CreateEventMessage(Checkout checkout, ShoppingCart basket)
    {
        return new()
        {
            AddressLine = checkout.AddressLine,
            CardName = checkout.CardName,
            CardNumber = checkout.CardNumber,
            Country = checkout.Country,
            CustomerId = checkout.CustomerId,
            Cvv = checkout.Cvv,
            EmailAddress = checkout.EmailAddress,
            Expiration = checkout.Expiration,
            FirstName = checkout.FirstName,
            LastName = checkout.LastName,
            PaymentMethod = checkout.PaymentMethod,
            State = checkout.State,
            TotalPrice = basket.TotalPrice,
            Username = basket.Username,
            ZipCode = checkout.ZipCode,
            Items = basket.Items.Select(x => new BasketCheckoutItem
            {
                Price = x.Price,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
            }).ToList()
        };
    }
}
