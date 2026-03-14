namespace RestaurantSaas.Api.Domain.Entities;

public class ModifierOption
{
    public Guid Id { get; set; }
    public Guid ModifierGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; } = 0;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public ModifierGroup ModifierGroup { get; set; } = null!;
}
