namespace BuildingBlocks.MessageBroker.Events;

public record IntegrationEvent
{
    public static Guid Id => Guid.NewGuid();
    public static DateTime OccurredOn => DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}
