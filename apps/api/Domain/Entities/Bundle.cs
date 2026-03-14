namespace RestaurantSaas.Api.Domain.Entities;

public class Bundle
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<BundleSlot> Slots { get; set; } = [];
}
