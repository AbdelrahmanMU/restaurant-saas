namespace RestaurantSaas.Api.Domain.Entities;

public class DriverBranchAccess
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Guid BranchId { get; set; }

    public Driver Driver { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
