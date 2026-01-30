namespace BuildingBlocks.MessageBroker.Events;

public record UserDeletedEvent : IntegrationEvent
{
    public string UserId { get; init; } = default!;
    public string? Email { get; init; }
}
