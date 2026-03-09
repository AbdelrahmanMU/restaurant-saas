namespace RestaurantSaas.Api.Domain.Entities;

public class RestaurantSettings
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string DefaultCurrency { get; set; } = "SAR";
    public string TimeZone { get; set; } = "Asia/Riyadh";
    public bool AutoAcceptOrders { get; set; }

    public Restaurant Restaurant { get; set; } = null!;
}
