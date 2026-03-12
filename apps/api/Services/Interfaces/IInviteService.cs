using RestaurantSaas.Api.DTOs.Invites;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface IInviteService
{
    Task<(CreateInviteResponse? Result, string? Error)> CreateInviteAsync(
        CreateInviteRequest request,
        Guid restaurantId,
        string baseUrl);
}
