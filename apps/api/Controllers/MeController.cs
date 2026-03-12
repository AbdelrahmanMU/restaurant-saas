using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("me")]
[Authorize]
public class MeController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMe()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.PhoneNumber,
            Roles = user.UserRoles.Select(r => r.Role.ToString()).ToArray(),
            user.BranchId,
            user.RestaurantId
        });
    }
}
