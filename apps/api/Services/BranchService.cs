using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Branches;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class BranchService(AppDbContext db) : IBranchService
{
    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<BranchSummaryDto>> GetBranchesAsync(
        Guid restaurantId, Guid? scopeBranchId = null)
    {
        var query = db.Branches
            .Where(b => b.RestaurantId == restaurantId);

        if (scopeBranchId.HasValue)
            query = query.Where(b => b.Id == scopeBranchId.Value);

        var branches = await query.OrderBy(b => b.Name).ToListAsync();

        // Load all managers in one query to avoid N+1
        var branchIds = branches.Select(b => b.Id).ToList();
        var managerRoles = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role == Role.BranchManager
                      && ur.BranchId.HasValue
                      && branchIds.Contains(ur.BranchId!.Value))
            .ToListAsync();

        var managerByBranch = managerRoles
            .GroupBy(ur => ur.BranchId!.Value)
            .ToDictionary(g => g.Key, g => g.First().User);

        return branches.Select(b => ToDto(b,
            managerByBranch.TryGetValue(b.Id, out var m) ? m : null));
    }

    public async Task<BranchSummaryDto?> GetBranchAsync(Guid id, Guid restaurantId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && b.RestaurantId == restaurantId);

        if (branch is null) return null;

        var manager = await GetManagerUserAsync(id);
        return ToDto(branch, manager);
    }

    // ─── Mutations ─────────────────────────────────────────────────────────────

    public async Task<BranchSummaryDto> CreateBranchAsync(
        CreateBranchRequest request, Guid restaurantId)
    {
        var branch = new Branch
        {
            Id           = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name         = request.Name.Trim(),
            Address      = request.Address?.Trim() ?? string.Empty,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        return ToDto(branch, null);
    }

    public async Task<(BranchSummaryDto? Branch, string? Error)> UpdateBranchAsync(
        Guid id, UpdateBranchRequest request, Guid restaurantId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && b.RestaurantId == restaurantId);

        if (branch is null) return (null, "NOT_FOUND");

        branch.Name     = request.Name.Trim();
        branch.Address  = request.Address?.Trim() ?? string.Empty;
        branch.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        var manager = await GetManagerUserAsync(id);
        return (ToDto(branch, manager), null);
    }

    public async Task<(BranchSummaryDto? Branch, string? Error)> AssignManagerAsync(
        Guid branchId, Guid userId, Guid restaurantId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId);
        if (branch is null) return (null, "BRANCH_NOT_FOUND");

        var newManager = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.RestaurantId == restaurantId);
        if (newManager is null) return (null, "USER_NOT_FOUND");

        // Remove existing BranchManager role for this branch (replace, don't stack)
        var existing = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role == Role.BranchManager && ur.BranchId == branchId)
            .ToListAsync();

        foreach (var old in existing)
        {
            db.UserRoles.Remove(old);

            // Clear home branch if user has no other BranchManager assignments
            if (old.User is not null && old.User.Id != userId)
            {
                var hasOtherBM = await db.UserRoles.AnyAsync(ur =>
                    ur.UserId == old.User.Id &&
                    ur.Role == Role.BranchManager &&
                    ur.Id != old.Id);

                if (!hasOtherBM)
                    old.User.BranchId = null;
            }
        }

        // Assign new manager
        db.UserRoles.Add(new UserRole
        {
            Id           = Guid.NewGuid(),
            UserId       = userId,
            Role         = Role.BranchManager,
            RestaurantId = restaurantId,
            BranchId     = branchId,
            AssignedAt   = DateTime.UtcNow
        });

        newManager.BranchId = branchId;

        await db.SaveChangesAsync();

        return (ToDto(branch, newManager), null);
    }

    public async Task<(bool Success, string? Error)> RemoveManagerAsync(
        Guid branchId, Guid restaurantId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId);
        if (branch is null) return (false, "BRANCH_NOT_FOUND");

        var managerRoles = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role == Role.BranchManager && ur.BranchId == branchId)
            .ToListAsync();

        if (managerRoles.Count == 0) return (false, "NO_MANAGER");

        foreach (var role in managerRoles)
        {
            db.UserRoles.Remove(role);

            if (role.User is not null)
            {
                var hasOtherBM = await db.UserRoles.AnyAsync(ur =>
                    ur.UserId == role.User.Id &&
                    ur.Role == Role.BranchManager &&
                    ur.Id != role.Id);

                if (!hasOtherBM)
                    role.User.BranchId = null;
            }
        }

        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private async Task<User?> GetManagerUserAsync(Guid branchId)
    {
        var role = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.Role == Role.BranchManager && ur.BranchId == branchId)
            .FirstOrDefaultAsync();

        return role?.User;
    }

    private static BranchSummaryDto ToDto(Branch branch, User? manager) => new(
        branch.Id,
        branch.Name,
        branch.Address,
        branch.IsActive,
        branch.CreatedAt,
        manager is null ? null : new BranchManagerDto(manager.Id, manager.FullName, manager.PhoneNumber)
    );
}
