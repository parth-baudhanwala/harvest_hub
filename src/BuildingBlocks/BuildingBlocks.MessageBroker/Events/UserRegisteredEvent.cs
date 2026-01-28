namespace BuildingBlocks.MessageBroker.Events;

public record UserRegisteredEvent : IntegrationEvent
{
    public string UserId { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
}
