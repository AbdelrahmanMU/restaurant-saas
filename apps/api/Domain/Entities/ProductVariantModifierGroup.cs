namespace RestaurantSaas.Api.Domain.Entities;

public class ProductVariantModifierGroup
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ModifierGroupId { get; set; }
    public int SortOrder { get; set; } = 0;

    public ProductVariant ProductVariant { get; set; } = null!;
    public ModifierGroup ModifierGroup { get; set; } = null!;
}
