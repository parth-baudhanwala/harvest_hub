using OrderStream.Domain.Enums;

namespace OrderStream.Application.Dtos;

public record OrderDto(Guid Id,
                       Guid CustomerId,
                       string Name,
                       AddressDto ShippingAddress,
                       AddressDto BillingAddress,
                       PaymentDto Payment,
                       OrderStatus Status,
                       List<OrderItemDto> Items);
