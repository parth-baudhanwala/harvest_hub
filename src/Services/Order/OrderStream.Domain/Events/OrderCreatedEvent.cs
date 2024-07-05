namespace OrderStream.Domain.Events;

public record OrderCreatedEvent(Order Order) : IDomainEvent;
