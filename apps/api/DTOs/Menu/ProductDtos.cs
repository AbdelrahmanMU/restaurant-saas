namespace RestaurantSaas.Api.DTOs.Menu;

public record ProductSummaryDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    Guid? MenuSectionId,
    string? MenuSectionName,
    string Name,
    string? Description,
    string? ImageUrl,
    string Type,
    int SortOrder,
    bool IsActive,
    decimal? BasePrice,
    int VariantCount,
    DateTime CreatedAt
);

public record ProductDetailDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    Guid? MenuSectionId,
    string? MenuSectionName,
    string Name,
    string? Description,
    string? ImageUrl,
    string Type,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<VariantDto> Variants,
    BundleDto? Bundle
);

public record VariantDto(
    Guid Id,
    string Name,
    string? Sku,
    decimal Price,
    bool IsDefault,
    bool IsActive,
    int SortOrder,
    IEnumerable<LinkedModifierGroupDto> ModifierGroups
);

public record LinkedModifierGroupDto(
    Guid LinkId,
    Guid ModifierGroupId,
    string Name,
    string SelectionType,
    bool IsRequired,
    int MinSelections,
    int? MaxSelections,
    int SortOrder,
    IEnumerable<ModifierOptionDto> Options
);

public record CreateProductRequest(
    Guid? BranchId,
    Guid? MenuSectionId,
    string Name,
    string? Description,
    string? ImageUrl,
    string Type,
    int SortOrder = 0
);

public record UpdateProductRequest(
    Guid? MenuSectionId,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive
);

public record CreateVariantRequest(
    string Name,
    string? Sku,
    decimal Price,
    bool IsDefault,
    int SortOrder = 0
);

public record UpdateVariantRequest(
    string Name,
    string? Sku,
    decimal Price,
    bool IsDefault,
    int SortOrder,
    bool IsActive
);

public record LinkModifierGroupRequest(
    Guid ModifierGroupId,
    int SortOrder = 0
);

public record ProductVariantSummaryDto(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string VariantName,
    decimal Price
);
