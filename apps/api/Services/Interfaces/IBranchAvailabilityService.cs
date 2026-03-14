using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IBranchAvailabilityService
{
    Task<IEnumerable<BranchVariantDto>> GetAvailabilityAsync(Guid restaurantId, Guid branchId);
    Task<BranchVariantDto> UpsertAsync(Guid productVariantId, UpsertBranchVariantRequest request, Guid restaurantId, Guid branchId);
}
