namespace RestaurantSaas.Api.Domain.Entities;

public class BundleSlot
{
    public Guid Id { get; set; }
    public Guid BundleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public int MinChoices { get; set; } = 1;
    public int MaxChoices { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Bundle Bundle { get; set; } = null!;
    public ICollection<BundleSlotChoice> Choices { get; set; } = [];
}
