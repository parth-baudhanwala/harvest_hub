namespace OrderStream.Domain.Events;

public record OrderUpdatedEvent(Order Order) : IDomainEvent;
