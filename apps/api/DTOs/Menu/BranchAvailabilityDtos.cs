namespace RestaurantSaas.Api.DTOs.Menu;

public record BranchVariantDto(
    Guid Id,
    Guid BranchId,
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    string? MenuSectionName,
    decimal BasePrice,
    bool IsAvailable,
    decimal? PriceOverride
);

public record UpsertBranchVariantRequest(
    bool IsAvailable,
    decimal? PriceOverride = null
);
