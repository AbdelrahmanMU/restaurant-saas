namespace RestaurantSaas.Api.Domain.Entities;

public class DriverDocument
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Driver Driver { get; set; } = null!;
}
