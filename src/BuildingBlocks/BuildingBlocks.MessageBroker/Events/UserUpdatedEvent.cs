namespace BuildingBlocks.MessageBroker.Events;

public record UserUpdatedEvent : IntegrationEvent
{
    public string UserId { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
}
