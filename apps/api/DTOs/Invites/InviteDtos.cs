namespace RestaurantSaas.Api.DTOs.Invites;

public record CreateInviteRequest(
    string PhoneNumber,
    string Role,
    Guid? BranchId
);

public record CreateInviteResponse(
    Guid InviteId,
    string ActivationLink,
    DateTime ExpiresAt
);
