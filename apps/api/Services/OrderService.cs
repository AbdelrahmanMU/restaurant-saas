using RestaurantSaas.Api.DTOs.Orders;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class OrderService : IOrderService
{
    // Mock in-memory orders — replace with DB queries when ready
    private static readonly List<OrderDto> _mockOrders =
    [
        new(Guid.NewGuid(), "#001", "PendingAcceptance", "DineIn",    "طاولة 1", DateTime.UtcNow.AddMinutes(-15)),
        new(Guid.NewGuid(), "#002", "PendingAcceptance", "Delivery",  null,       DateTime.UtcNow.AddMinutes(-8)),
        new(Guid.NewGuid(), "#003", "Accepted",          "DineIn",    "طاولة 3", DateTime.UtcNow.AddMinutes(-30)),
        new(Guid.NewGuid(), "#004", "Preparing",         "Pickup",    null,       DateTime.UtcNow.AddMinutes(-45)),
        new(Guid.NewGuid(), "#005", "PendingAcceptance", "DineIn",    "طاولة 5", DateTime.UtcNow.AddMinutes(-2)),
    ];

    public List<OrderDto> GetOrders() => _mockOrders;
}
