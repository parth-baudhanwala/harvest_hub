using BuildingBlocks.Exceptions;

namespace OrderStream.Application.Exceptions;

public class OrderNotFoundException(Guid id) : NotFoundException("Order", id);
