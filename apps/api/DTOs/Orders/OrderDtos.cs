namespace RestaurantSaas.Api.DTOs.Orders;

public record OrderDto(
    Guid Id,
    string Number,
    string Status,
    string Type,
    string? TableNumber,
    DateTime CreatedAt
);
