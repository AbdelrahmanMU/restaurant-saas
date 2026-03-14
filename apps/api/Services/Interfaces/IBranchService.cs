using RestaurantSaas.Api.DTOs.Branches;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchSummaryDto>> GetBranchesAsync(Guid restaurantId, Guid? scopeBranchId = null);

    Task<BranchSummaryDto?> GetBranchAsync(Guid id, Guid restaurantId);

    Task<BranchSummaryDto> CreateBranchAsync(CreateBranchRequest request, Guid restaurantId);

    Task<(BranchSummaryDto? Branch, string? Error)> UpdateBranchAsync(
        Guid id, UpdateBranchRequest request, Guid restaurantId);

    Task<(BranchSummaryDto? Branch, string? Error)> AssignManagerAsync(
        Guid branchId, Guid userId, Guid restaurantId);

    Task<(bool Success, string? Error)> RemoveManagerAsync(Guid branchId, Guid restaurantId);
}
