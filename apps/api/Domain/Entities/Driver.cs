namespace RestaurantSaas.Api.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<DriverAttribute> Attributes { get; set; } = [];
    public ICollection<DriverDocument> Documents { get; set; } = [];
    public ICollection<DriverBranchAccess> BranchAccess { get; set; } = [];
}
