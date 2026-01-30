namespace BuildingBlocks.MessageBroker.Events;

public record AdminUserUpsertedEvent : IntegrationEvent
{
    public string UserId { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
    public bool IsAdmin { get; init; }
}
