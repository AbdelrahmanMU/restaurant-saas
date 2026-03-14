using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Domain.Entities;

public class ModifierGroup
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SelectionType SelectionType { get; set; } = SelectionType.Single;
    public bool IsRequired { get; set; } = false;
    public int MinSelections { get; set; } = 0;
    public int? MaxSelections { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Restaurant Restaurant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<ModifierOption> Options { get; set; } = [];
    public ICollection<ProductVariantModifierGroup> ProductVariants { get; set; } = [];
}
