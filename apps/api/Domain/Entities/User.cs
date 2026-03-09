using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? RestaurantId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }
}
