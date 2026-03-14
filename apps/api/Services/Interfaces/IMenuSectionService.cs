using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IMenuSectionService
{
    Task<IEnumerable<MenuSectionDto>> GetSectionsAsync(Guid restaurantId, Guid? branchId = null);
    Task<MenuSectionDto?> GetSectionAsync(Guid id, Guid restaurantId, Guid? branchId = null);
    Task<(MenuSectionDto? Section, string? Error)> CreateSectionAsync(CreateMenuSectionRequest request, Guid restaurantId, Guid branchId);
    Task<(MenuSectionDto? Section, string? Error)> UpdateSectionAsync(Guid id, UpdateMenuSectionRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeactivateSectionAsync(Guid id, Guid restaurantId, Guid? branchId = null);
}
