namespace RestaurantSaas.Api.Domain.Entities;

public class DriverAttribute
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Driver Driver { get; set; } = null!;
}
