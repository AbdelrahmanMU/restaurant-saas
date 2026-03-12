using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(
        Guid userId,
        string phoneNumber,
        IEnumerable<Role> roles,
        string fullName,
        Guid? branchId = null,
        Guid? restaurantId = null)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.MobilePhone, phoneNumber),
            new(ClaimTypes.Name, fullName)
        };

        // One ClaimTypes.Role claim per role — ASP.NET Core evaluates them with OR logic
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        if (branchId.HasValue)
            claims.Add(new Claim("branch_id", branchId.Value.ToString()));

        if (restaurantId.HasValue)
            claims.Add(new Claim("restaurant_id", restaurantId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
