using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Invites;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("invites")]
[Authorize(Roles = "Owner,BranchManager")]
public class InvitesController(IInviteService inviteService, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateInvite([FromBody] CreateInviteRequest request)
    {
        var restaurantIdClaim = User.FindFirstValue("restaurant_id");

        if (string.IsNullOrEmpty(restaurantIdClaim) || !Guid.TryParse(restaurantIdClaim, out var restaurantId))
            return Forbid();

        var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";

        var (result, error) = await inviteService.CreateInviteAsync(request, restaurantId, frontendBaseUrl);

        return error switch
        {
            "PHONE_TAKEN" => Conflict(new { message = "رقم الهاتف مسجّل بالفعل" }),
            "INVALID_ROLE" => BadRequest(new { message = "الدور غير صالح" }),
            _ => Ok(result)
        };
    }
}
