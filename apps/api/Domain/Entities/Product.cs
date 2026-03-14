using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? MenuSectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ProductType Type { get; set; } = ProductType.Simple;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public MenuSection? MenuSection { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public Bundle? Bundle { get; set; }
}
