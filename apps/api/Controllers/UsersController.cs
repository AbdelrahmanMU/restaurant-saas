using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Users;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    // ─── JWT helpers ───────────────────────────────────────────────────────────

    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    private Guid? CallerBranchId =>
        Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;

    // ASP.NET Core maps JWT "sub" → ClaimTypes.NameIdentifier
    private Guid? CallerId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>
    /// Returns the caller's highest-authority role regardless of how many roles they hold.
    /// Owner always wins; then RestaurantManager; then BranchManager; otherwise operational.
    /// </summary>
    private Role CallerEffectiveRole
    {
        get
        {
            if (User.IsInRole("Owner"))             return Role.Owner;
            if (User.IsInRole("RestaurantManager")) return Role.RestaurantManager;
            if (User.IsInRole("BranchManager"))     return Role.BranchManager;
            return Role.Cashier; // Cashier / Coordinator / Driver share level 3
        }
    }

    // ─── Role hierarchy ────────────────────────────────────────────────────────

    /// <summary>
    /// Lower number = higher authority. Owner = 0, operational = 3.
    /// </summary>
    private static int RoleLevel(Role role) => role switch
    {
        Role.Owner             => 0,
        Role.RestaurantManager => 1,
        Role.BranchManager     => 2,
        _                      => 3   // Cashier, Coordinator, Driver
    };

    /// <summary>Returns the authority level of the user's most senior role.</summary>
    private static int EffectiveLevel(ICollection<UserRole> roles) =>
        roles.Count > 0 ? roles.Min(r => RoleLevel(r.Role)) : 99;

    // ─── GET /users ────────────────────────────────────────────────────────────
    // Returns two lists:
    //   manageable — employees the caller may edit / assign roles
    //   managers   — the caller's own superiors (read-only, informational)
    // Peers (same effective level) are never included.
    // The caller themselves is always excluded.
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var restaurantId = RestaurantId;
        var callerId     = CallerId;
        if (restaurantId is null || callerId is null) return Forbid();

        var callerRole  = CallerEffectiveRole;
        var callerLevel = RoleLevel(callerRole);
        var branchId    = CallerBranchId;

        var allUsers = await db.Users
            .Include(u => u.UserRoles)
            .Where(u => u.RestaurantId == restaurantId && u.Id != callerId)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var manageable = new List<User>();
        var managers   = new List<User>();

        foreach (var u in allUsers)
        {
            var userLevel = EffectiveLevel(u.UserRoles);

            if (userLevel > callerLevel)
            {
                // This user is a subordinate — enforce branch scope for BranchManager callers
                if (callerLevel >= 2 && u.BranchId != branchId) continue;
                manageable.Add(u);
            }
            else if (userLevel < callerLevel)
            {
                // This user is a superior (read-only for the caller)
                // BranchManager-level superiors: only show if same branch
                if (userLevel == 2 && u.BranchId != branchId) continue;
                managers.Add(u);
            }
            // Peers (same level) are not shown in either list
        }

        return Ok(new UsersListDto(
            manageable.Select(ToSummaryDto).ToArray(),
            managers.Select(ToSummaryDto).ToArray()
        ));
    }

    // ─── GET /users/{id} ──────────────────────────────────────────────────────
    // Accessible for both subordinates (full detail) and superiors (read-only detail).
    // Self is always blocked.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var restaurantId = RestaurantId;
        var callerId     = CallerId;
        if (restaurantId is null || callerId is null) return Forbid();

        if (id == callerId) return Forbid();

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

        if (user is null) return NotFound();

        var callerLevel = RoleLevel(CallerEffectiveRole);
        var userLevel   = EffectiveLevel(user.UserRoles);
        var branchId    = CallerBranchId;

        var canAccess = userLevel > callerLevel
            // subordinate: branch-scope for BranchManager and below
            ? callerLevel < 2 || user.BranchId == branchId
            // superior: branch-scope only for BranchManager-level superiors
            : userLevel < callerLevel && (userLevel != 2 || user.BranchId == branchId);

        if (!canAccess) return Forbid();

        return Ok(new UserDetailDto(
            user.Id,
            user.FullName,
            user.PhoneNumber,
            user.UserRoles.Select(r => new UserRoleEntryDto(r.Id, r.Role.ToString(), r.BranchId)).ToArray(),
            user.BranchId,
            user.IsActive,
            user.CreatedAt
        ));
    }

    // ─── POST /users/{id}/roles ────────────────────────────────────────────────
    // Only Owner, RestaurantManager, BranchManager may add roles.
    // Caller can only assign roles strictly below their own level.
    // Owner role can never be assigned here (system-only via registration).
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
    public async Task<IActionResult> AddRole(Guid id, [FromBody] AddRoleRequest request)
    {
        var restaurantId = RestaurantId;
        var callerId     = CallerId;
        if (restaurantId is null || callerId is null) return Forbid();

        if (id == callerId) return Forbid();

        if (!Enum.TryParse<Role>(request.Role, out var role))
            return BadRequest(new { message = "دور غير صالح" });

        // Owner role is system-established only — never assignable through Employee Management
        if (role == Role.Owner)
            return BadRequest(new { message = "لا يمكن تعيين دور المالك من هنا" });

        var callerLevel = RoleLevel(CallerEffectiveRole);

        // Can only assign roles strictly below caller's level
        if (RoleLevel(role) <= callerLevel)
            return Forbid();

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

        if (user is null) return NotFound();

        // Target must be a subordinate (not a peer or superior)
        if (EffectiveLevel(user.UserRoles) <= callerLevel) return Forbid();

        // Branch scope for BranchManager callers
        if (callerLevel >= 2)
        {
            var branchId = CallerBranchId;
            if (branchId is null || user.BranchId != branchId) return Forbid();
        }

        var alreadyHas = user.UserRoles.Any(r => r.Role == role && r.BranchId == request.BranchId);
        if (alreadyHas)
            return Conflict(new { message = "المستخدم لديه هذا الدور بالفعل" });

        var userRole = new UserRole
        {
            Id           = Guid.NewGuid(),
            UserId       = user.Id,
            Role         = role,
            RestaurantId = restaurantId,
            BranchId     = request.BranchId,
            AssignedAt   = DateTime.UtcNow
        };

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        return Ok(new UserRoleEntryDto(userRole.Id, userRole.Role.ToString(), userRole.BranchId));
    }

    // ─── DELETE /users/{id}/roles/{roleId} ────────────────────────────────────
    // Only Owner, RestaurantManager, BranchManager may remove roles.
    // Caller can only remove roles strictly below their own level.
    // Owner role is immutable — can never be removed through Employee Management.
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
    {
        var restaurantId = RestaurantId;
        var callerId     = CallerId;
        if (restaurantId is null || callerId is null) return Forbid();

        if (id == callerId) return Forbid();

        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.UserId == id && r.RestaurantId == restaurantId);

        if (userRole is null) return NotFound();

        // Owner role is immutable through Employee Management — for any caller
        if (userRole.Role == Role.Owner)
            return BadRequest(new { message = "لا يمكن إزالة دور المالك من هنا" });

        var callerLevel = RoleLevel(CallerEffectiveRole);

        // Can only remove roles strictly below caller's level
        if (RoleLevel(userRole.Role) <= callerLevel)
            return Forbid();

        // Target user must be a subordinate (not a peer or superior)
        var targetUser = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

        if (targetUser is null) return NotFound();
        if (EffectiveLevel(targetUser.UserRoles) <= callerLevel) return Forbid();

        // Branch scope for BranchManager callers
        if (callerLevel >= 2)
        {
            var branchId = CallerBranchId;
            if (branchId is null || targetUser.BranchId != branchId) return Forbid();
        }

        var totalRoles = await db.UserRoles.CountAsync(r => r.UserId == id);
        if (totalRoles <= 1)
            return BadRequest(new { message = "لا يمكن حذف الدور الوحيد للمستخدم" });

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // ─── Private helper ────────────────────────────────────────────────────────

    private static UserSummaryDto ToSummaryDto(User u) => new(
        u.Id,
        u.FullName,
        u.PhoneNumber,
        u.UserRoles.Select(r => r.Role.ToString()).ToArray(),
        u.BranchId,
        u.IsActive,
        u.CreatedAt
    );
}
