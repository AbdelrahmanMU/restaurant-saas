using RestaurantSaas.Api.DTOs.Auth;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(LoginResponse? Result, string? Error)> RegisterOwnerAsync(RegisterOwnerRequest request);
    Task<LoginResponse?> ActivateInviteAsync(ActivateInviteRequest request);
}
