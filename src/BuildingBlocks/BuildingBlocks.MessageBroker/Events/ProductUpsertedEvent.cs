namespace BuildingBlocks.MessageBroker.Events;

public record ProductUpsertedEvent : IntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
}
