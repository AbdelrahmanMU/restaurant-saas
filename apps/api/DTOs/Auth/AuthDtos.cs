namespace RestaurantSaas.Api.DTOs.Auth;

public record LoginRequest(string PhoneNumber, string Password);

public record LoginResponse(
    string Token,
    string Role,
    string FullName,
    Guid? BranchId,
    Guid? RestaurantId
);

public record RegisterOwnerRequest(
    string FullName,
    string PhoneNumber,
    string Password,
    string RestaurantName,
    string? BranchName = null
);

public record ActivateInviteRequest(
    string InviteToken,
    string FullName,
    string PhoneNumber,
    string Password
);
