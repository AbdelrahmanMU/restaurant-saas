using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IBundleService
{
    Task<BundleDto?> GetBundleAsync(Guid productId, Guid restaurantId, Guid? branchId = null);
    Task<(BundleDto? Bundle, string? Error)> EnsureBundleAsync(Guid productId, Guid restaurantId, Guid? branchId = null);

    // Slots
    Task<(BundleSlotDto? Slot, string? Error)> AddSlotAsync(Guid productId, CreateBundleSlotRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(BundleSlotDto? Slot, string? Error)> UpdateSlotAsync(Guid productId, Guid slotId, UpdateBundleSlotRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeleteSlotAsync(Guid productId, Guid slotId, Guid restaurantId, Guid? branchId = null);

    // Choices
    Task<(BundleSlotChoiceDto? Choice, string? Error)> AddChoiceAsync(Guid productId, Guid slotId, AddBundleSlotChoiceRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(BundleSlotChoiceDto? Choice, string? Error)> UpdateChoiceAsync(Guid productId, Guid slotId, Guid choiceId, UpdateBundleSlotChoiceRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> RemoveChoiceAsync(Guid productId, Guid slotId, Guid choiceId, Guid restaurantId, Guid? branchId = null);
}
