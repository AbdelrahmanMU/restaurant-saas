using RestaurantSaas.Api.Domain.Enums;

namespace RestaurantSaas.Api.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(
        Guid userId,
        string phoneNumber,
        IEnumerable<Role> roles,
        string fullName,
        Guid? branchId = null,
        Guid? restaurantId = null);
}
