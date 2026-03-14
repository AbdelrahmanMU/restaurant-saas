namespace RestaurantSaas.Api.DTOs.Menu;

public record ModifierGroupDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    string SelectionType,
    bool IsRequired,
    int MinSelections,
    int? MaxSelections,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<ModifierOptionDto> Options
);

public record ModifierGroupSummaryDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    string SelectionType,
    bool IsRequired,
    int MinSelections,
    int? MaxSelections,
    int SortOrder,
    bool IsActive
);

public record ModifierOptionDto(
    Guid Id,
    string Name,
    decimal PriceDelta,
    bool IsDefault,
    bool IsActive,
    int SortOrder
);

public record CreateModifierGroupRequest(
    Guid? BranchId,
    string Name,
    string SelectionType,
    bool IsRequired,
    int MinSelections = 0,
    int? MaxSelections = null,
    int SortOrder = 0
);

public record UpdateModifierGroupRequest(
    string Name,
    string SelectionType,
    bool IsRequired,
    int MinSelections,
    int? MaxSelections,
    int SortOrder,
    bool IsActive
);

public record CreateModifierOptionRequest(
    string Name,
    decimal PriceDelta = 0,
    bool IsDefault = false,
    int SortOrder = 0
);

public record UpdateModifierOptionRequest(
    string Name,
    decimal PriceDelta,
    bool IsDefault,
    int SortOrder,
    bool IsActive
);
