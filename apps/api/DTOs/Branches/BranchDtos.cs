namespace RestaurantSaas.Api.DTOs.Branches;

public record BranchSummaryDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive,
    DateTime CreatedAt,
    BranchManagerDto? Manager
);

public record BranchManagerDto(
    Guid UserId,
    string FullName,
    string PhoneNumber
);

public record CreateBranchRequest(
    string Name,
    string Address
);

public record UpdateBranchRequest(
    string Name,
    string Address,
    bool IsActive
);

public record AssignManagerRequest(
    Guid UserId
);
