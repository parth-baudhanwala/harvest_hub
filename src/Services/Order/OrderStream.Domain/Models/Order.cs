namespace OrderStream.Domain.Models;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public CustomerId CustomerId { get; private set; } = default!;
    public OrderName Name { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public Address BillingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalPrice
    {
        get => Items.Sum(x => x.Price * x.Quantity);
        private set
        {
            // Total Price should not set.
        }
    }

    public static Order Create(OrderId id, CustomerId customerId, OrderName name, Address shippingAddress, Address billingAddress, Payment payment)
    {
        Order order = new()
        {
            Id = id,
            CustomerId = customerId,
            Name = name,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending
        };

        order.AddDomainEvent(new OrderCreatedEvent(order));

        return order;
    }

    public void Update(OrderName name, Address shippingAddress, Address billingAddress, Payment payment, OrderStatus status)
    {
        Name = name;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Payment = payment;
        Status = status;

        AddDomainEvent(new OrderUpdatedEvent(this));
    }

    public void Add(ProductId productId, int quantity, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        var orderItem = new OrderItem(Id, productId, quantity, price);
        _items.Add(orderItem);
    }

    public void Remove(ProductId productId)
    {
        var orderItem = _items.Find(x => x.ProductId == productId);

        if (orderItem is not null)
        {
            _items.Remove(orderItem);
        }
    }
}
