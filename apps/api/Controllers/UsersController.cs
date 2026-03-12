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
[Authorize(Roles = "Owner")]
public class UsersController(AppDbContext db) : ControllerBase
{
    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    // GET /users  — all users in the same restaurant
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var users = await db.Users
            .Include(u => u.UserRoles)
            .Where(u => u.RestaurantId == restaurantId)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = users.Select(u => new UserSummaryDto(
            u.Id,
            u.FullName,
            u.PhoneNumber,
            u.UserRoles.Select(r => r.Role.ToString()).ToArray(),
            u.BranchId,
            u.IsActive,
            u.CreatedAt
        ));

        return Ok(result);
    }

    // GET /users/{id}  — user detail with role entries (includes role GUIDs for removal)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

        if (user is null) return NotFound();

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

    // POST /users/{id}/roles  — add a role to a user
    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AddRole(Guid id, [FromBody] AddRoleRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        if (!Enum.TryParse<Role>(request.Role, out var role))
            return BadRequest(new { message = "دور غير صالح" });

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

        if (user is null) return NotFound();

        var alreadyHas = user.UserRoles.Any(r => r.Role == role && r.BranchId == request.BranchId);
        if (alreadyHas)
            return Conflict(new { message = "المستخدم لديه هذا الدور بالفعل" });

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = role,
            RestaurantId = restaurantId,
            BranchId = request.BranchId,
            AssignedAt = DateTime.UtcNow
        };

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        return Ok(new UserRoleEntryDto(userRole.Id, userRole.Role.ToString(), userRole.BranchId));
    }

    // DELETE /users/{id}/roles/{roleId}  — remove a specific role assignment
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.UserId == id && r.RestaurantId == restaurantId);

        if (userRole is null) return NotFound();

        // Prevent removing the last role
        var totalRoles = await db.UserRoles.CountAsync(r => r.UserId == id);
        if (totalRoles <= 1)
            return BadRequest(new { message = "لا يمكن حذف الدور الوحيد للمستخدم" });

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
