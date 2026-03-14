namespace RestaurantSaas.Api.DTOs.Menu;

public record MenuSectionDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateMenuSectionRequest(
    Guid? BranchId,
    string Name,
    string? Description,
    int SortOrder = 0
);

public record UpdateMenuSectionRequest(
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive
);
