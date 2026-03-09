namespace RestaurantSaas.Api.Domain.Entities;

public class DriverActiveOrder
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Driver Driver { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
