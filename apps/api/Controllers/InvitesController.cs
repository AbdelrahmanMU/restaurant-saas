using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Invites;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("invites")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class InvitesController(IInviteService inviteService, IConfiguration configuration) : ControllerBase
{
    // Role hierarchy: lower number = higher authority
    private static int RoleLevel(Role role) => role switch
    {
        Role.Owner             => 0,
        Role.RestaurantManager => 1,
        Role.BranchManager     => 2,
        _                      => 3
    };

    private Role CallerEffectiveRole
    {
        get
        {
            if (User.IsInRole("Owner"))             return Role.Owner;
            if (User.IsInRole("RestaurantManager")) return Role.RestaurantManager;
            return Role.BranchManager;
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvite([FromBody] CreateInviteRequest request)
    {
        var restaurantIdClaim = User.FindFirstValue("restaurant_id");
        if (string.IsNullOrEmpty(restaurantIdClaim) || !Guid.TryParse(restaurantIdClaim, out var restaurantId))
            return Forbid();

        // Validate requested role exists
        if (!Enum.TryParse<Role>(request.Role, out var requestedRole))
            return BadRequest(new { message = "الدور غير صالح" });

        // Owner role is system-established only — never invitable
        if (requestedRole == Role.Owner)
            return BadRequest(new { message = "لا يمكن دعوة شخص لدور المالك" });

        var callerLevel = RoleLevel(CallerEffectiveRole);

        // Caller can only invite roles strictly below their own level
        if (RoleLevel(requestedRole) <= callerLevel)
            return Forbid();

        // Non-Owner callers: enforce their own branch on the invite
        Guid? effectiveBranchId = request.BranchId;
        if (CallerEffectiveRole != Role.Owner)
        {
            var branchIdClaim = User.FindFirstValue("branch_id");
            if (string.IsNullOrEmpty(branchIdClaim) || !Guid.TryParse(branchIdClaim, out var callerBranchId))
                return Forbid();

            effectiveBranchId = callerBranchId;
        }

        var finalRequest = request with { BranchId = effectiveBranchId };
        var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
        var (result, error) = await inviteService.CreateInviteAsync(finalRequest, restaurantId, frontendBaseUrl);

        return error switch
        {
            "PHONE_TAKEN" => Conflict(new { message = "رقم الهاتف مسجّل بالفعل" }),
            "INVALID_ROLE" => BadRequest(new { message = "الدور غير صالح" }),
            _ => Ok(result)
        };
    }
}
