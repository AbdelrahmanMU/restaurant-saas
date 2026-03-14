namespace RestaurantSaas.Api.Domain.Entities;

public class BranchProductVariant
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductVariantId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal? PriceOverride { get; set; }

    public Branch Branch { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
