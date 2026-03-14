namespace RestaurantSaas.Api.DTOs.Menu;

public record BundleDto(
    Guid Id,
    Guid ProductId,
    IEnumerable<BundleSlotDto> Slots
);

public record BundleSlotDto(
    Guid Id,
    string Name,
    bool IsRequired,
    int MinChoices,
    int MaxChoices,
    int SortOrder,
    IEnumerable<BundleSlotChoiceDto> Choices
);

public record BundleSlotChoiceDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    decimal BasePrice,
    decimal PriceDelta
);

public record CreateBundleSlotRequest(
    string Name,
    bool IsRequired = true,
    int MinChoices = 1,
    int MaxChoices = 1,
    int SortOrder = 0
);

public record UpdateBundleSlotRequest(
    string Name,
    bool IsRequired,
    int MinChoices,
    int MaxChoices,
    int SortOrder
);

public record AddBundleSlotChoiceRequest(
    Guid ProductVariantId,
    decimal PriceDelta = 0
);

public record UpdateBundleSlotChoiceRequest(
    decimal PriceDelta
);
