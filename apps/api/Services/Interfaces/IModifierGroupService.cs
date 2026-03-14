using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IModifierGroupService
{
    Task<IEnumerable<ModifierGroupDto>> GetGroupsAsync(Guid restaurantId, Guid? branchId = null);
    Task<ModifierGroupDto?> GetGroupAsync(Guid id, Guid restaurantId, Guid? branchId = null);
    Task<(ModifierGroupDto? Group, string? Error)> CreateGroupAsync(CreateModifierGroupRequest request, Guid restaurantId, Guid branchId);
    Task<(ModifierGroupDto? Group, string? Error)> UpdateGroupAsync(Guid id, UpdateModifierGroupRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeactivateGroupAsync(Guid id, Guid restaurantId, Guid? branchId = null);

    // Options
    Task<(ModifierOptionDto? Option, string? Error)> AddOptionAsync(Guid groupId, CreateModifierOptionRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(ModifierOptionDto? Option, string? Error)> UpdateOptionAsync(Guid groupId, Guid optionId, UpdateModifierOptionRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeactivateOptionAsync(Guid groupId, Guid optionId, Guid restaurantId, Guid? branchId = null);
}
