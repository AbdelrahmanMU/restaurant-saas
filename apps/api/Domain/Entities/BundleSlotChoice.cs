namespace RestaurantSaas.Api.Domain.Entities;

public class BundleSlotChoice
{
    public Guid Id { get; set; }
    public Guid BundleSlotId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal PriceDelta { get; set; } = 0;

    public BundleSlot BundleSlot { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
