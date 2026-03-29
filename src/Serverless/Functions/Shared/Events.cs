namespace Functions.Shared;

/// <summary>
/// Integration event models matching BuildingBlocks.MessageBroker events
/// </summary>
/// 
public record BasketCheckoutEvent
{
    public string Username { get; set; } = default!;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public decimal TotalPrice { get; set; } = default!;

    // Shipping and BillingAddress
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

    // Payment
    public string CardName { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public string CardExpiration { get; set; } = default!;
    public string CardSecurityNumber { get; set; } = default!;

    public int CardExpirationMonth { get; set; } = default!;
    public int CardExpirationYear { get; set; } = default!;

    // Timestamps
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}

public record ProductUpsertedEvent
{
    public Guid ProductId { get; set; } = Guid.Empty;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; } = default!;
    public int Stock { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}

public record UserRegisteredEvent
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}

public record OrderCreatedEvent
{
    public Guid OrderId { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public string CustomerEmail { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}

public record OrderUpdatedEvent
{
    public Guid OrderId { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public string Status { get; set; } = default!;
    public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
}

public record PaymentProcessedEvent
{
    public Guid OrderId { get; set; } = Guid.Empty;
    public string TransactionId { get; set; } = default!;
    public decimal Amount { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime ProcessedTime { get; set; } = DateTime.UtcNow;
}

public record InventoryReservedEvent
{
    public Guid OrderId { get; set; } = Guid.Empty;
    public Guid ProductId { get; set; } = Guid.Empty;
    public int Quantity { get; set; } = default!;
    public DateTime ReservedTime { get; set; } = DateTime.UtcNow;
}

public record UserUpdatedEvent
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Email { get; set; } = default!;
    public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
}

public record UserDeletedEvent
{
    public Guid UserId { get; set; } = Guid.Empty;
    public DateTime DeletedTime { get; set; } = DateTime.UtcNow;
}

public record AdminUserUpsertedEvent
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}
