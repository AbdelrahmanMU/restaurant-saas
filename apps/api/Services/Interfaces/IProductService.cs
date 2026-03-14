using RestaurantSaas.Api.DTOs.Menu;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IProductService
{
    // Products
    Task<IEnumerable<ProductSummaryDto>> GetProductsAsync(Guid restaurantId, Guid? branchId = null);
    Task<ProductDetailDto?> GetProductAsync(Guid id, Guid restaurantId, Guid? branchId = null);
    Task<(ProductDetailDto? Product, string? Error)> CreateProductAsync(CreateProductRequest request, Guid restaurantId, Guid branchId);
    Task<(ProductDetailDto? Product, string? Error)> UpdateProductAsync(Guid id, UpdateProductRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeactivateProductAsync(Guid id, Guid restaurantId, Guid? branchId = null);

    // Variants
    Task<IEnumerable<ProductVariantSummaryDto>> GetAllVariantsAsync(Guid restaurantId, Guid? branchId = null);
    Task<IEnumerable<VariantDto>> GetVariantsAsync(Guid productId, Guid restaurantId, Guid? branchId = null);
    Task<(VariantDto? Variant, string? Error)> CreateVariantAsync(Guid productId, CreateVariantRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(VariantDto? Variant, string? Error)> UpdateVariantAsync(Guid productId, Guid variantId, UpdateVariantRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> DeactivateVariantAsync(Guid productId, Guid variantId, Guid restaurantId, Guid? branchId = null);

    // Modifier Group Links
    Task<IEnumerable<LinkedModifierGroupDto>> GetVariantModifierGroupsAsync(Guid productId, Guid variantId, Guid restaurantId, Guid? branchId = null);
    Task<(LinkedModifierGroupDto? Link, string? Error)> LinkModifierGroupAsync(Guid productId, Guid variantId, LinkModifierGroupRequest request, Guid restaurantId, Guid? branchId = null);
    Task<(bool Success, string? Error)> UnlinkModifierGroupAsync(Guid productId, Guid variantId, Guid groupId, Guid restaurantId, Guid? branchId = null);
}
