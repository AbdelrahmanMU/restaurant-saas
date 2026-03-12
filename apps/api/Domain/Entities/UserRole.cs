using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Role Role { get; set; }
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
