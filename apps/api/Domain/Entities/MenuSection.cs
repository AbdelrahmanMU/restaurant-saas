namespace RestaurantSaas.Api.Domain.Entities;

public class MenuSection
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];
}
