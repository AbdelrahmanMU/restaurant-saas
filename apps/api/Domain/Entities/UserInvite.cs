using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Domain.Entities;

public class UserInvite
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
