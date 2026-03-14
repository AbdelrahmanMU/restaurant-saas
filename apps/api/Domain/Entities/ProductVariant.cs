namespace RestaurantSaas.Api.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public ICollection<ProductVariantModifierGroup> ModifierGroups { get; set; } = [];
    public ICollection<BranchProductVariant> BranchAvailability { get; set; } = [];
    public ICollection<BundleSlotChoice> BundleSlotChoices { get; set; } = [];
}
