namespace RestaurantSaas.Api.DTOs.Users;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string[] Roles,
    Guid? BranchId,
    bool IsActive,
    DateTime CreatedAt
);

public record UserDetailDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    UserRoleEntryDto[] RoleEntries,
    Guid? BranchId,
    bool IsActive,
    DateTime CreatedAt
);

public record UserRoleEntryDto(
    Guid Id,
    string Role,
    Guid? BranchId
);

public record AddRoleRequest(
    string Role,
    Guid? BranchId = null
);
