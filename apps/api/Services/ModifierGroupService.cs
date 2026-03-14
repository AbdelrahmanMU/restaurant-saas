using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class ModifierGroupService(AppDbContext db) : IModifierGroupService
{
    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ModifierGroupDto>> GetGroupsAsync(
        Guid restaurantId, Guid? branchId = null)
    {
        var query = db.ModifierGroups
            .Include(g => g.Branch)
            .Include(g => g.Options)
            .Where(g => g.RestaurantId == restaurantId && g.IsActive);

        if (branchId.HasValue)
            query = query.Where(g => g.BranchId == branchId.Value);

        var groups = await query
            .OrderBy(g => g.SortOrder)
            .ToListAsync();

        return groups.Select(g => ToDto(g));
    }

    public async Task<ModifierGroupDto?> GetGroupAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .Include(g => g.Branch)
            .Include(g => g.Options)
            .FirstOrDefaultAsync(g =>
                g.Id == id &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        return group is null ? null : ToDto(group);
    }

    // ─── Mutations ─────────────────────────────────────────────────────────────

    public async Task<(ModifierGroupDto? Group, string? Error)> CreateGroupAsync(
        CreateModifierGroupRequest request, Guid restaurantId, Guid branchId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId);
        if (branch is null) return (null, "BRANCH_NOT_FOUND");

        if (!Enum.TryParse<SelectionType>(request.SelectionType, out var selectionType))
            return (null, "INVALID_SELECTION_TYPE");

        var group = new ModifierGroup
        {
            Id            = Guid.NewGuid(),
            RestaurantId  = restaurantId,
            BranchId      = branchId,
            Name          = request.Name.Trim(),
            SelectionType = selectionType,
            IsRequired    = request.IsRequired,
            MinSelections = request.MinSelections,
            MaxSelections = request.MaxSelections,
            SortOrder     = request.SortOrder,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow,
            Branch        = branch,
            Options       = []
        };

        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();

        return (ToDto(group), null);
    }

    public async Task<(ModifierGroupDto? Group, string? Error)> UpdateGroupAsync(
        Guid id, UpdateModifierGroupRequest request, Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .Include(g => g.Branch)
            .Include(g => g.Options)
            .FirstOrDefaultAsync(g =>
                g.Id == id &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (null, "NOT_FOUND");

        if (!Enum.TryParse<SelectionType>(request.SelectionType, out var selectionType))
            return (null, "INVALID_SELECTION_TYPE");

        group.Name          = request.Name.Trim();
        group.SelectionType = selectionType;
        group.IsRequired    = request.IsRequired;
        group.MinSelections = request.MinSelections;
        group.MaxSelections = request.MaxSelections;
        group.SortOrder     = request.SortOrder;
        group.IsActive      = request.IsActive;

        await db.SaveChangesAsync();
        return (ToDto(group), null);
    }

    public async Task<(bool Success, string? Error)> DeactivateGroupAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .FirstOrDefaultAsync(g =>
                g.Id == id &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (false, "NOT_FOUND");

        var hasOptions = await db.ModifierOptions
            .AnyAsync(o => o.ModifierGroupId == id && o.IsActive);
        if (hasOptions) return (false, "GROUP_HAS_OPTIONS");

        var hasLinks = await db.ProductVariantModifierGroups
            .AnyAsync(pvmg => pvmg.ModifierGroupId == id);
        if (hasLinks) return (false, "GROUP_HAS_LINKS");

        group.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Options ───────────────────────────────────────────────────────────────

    public async Task<(ModifierOptionDto? Option, string? Error)> AddOptionAsync(
        Guid groupId, CreateModifierOptionRequest request, Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .FirstOrDefaultAsync(g =>
                g.Id == groupId &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (null, "GROUP_NOT_FOUND");

        var option = new ModifierOption
        {
            Id              = Guid.NewGuid(),
            ModifierGroupId = groupId,
            Name            = request.Name.Trim(),
            PriceDelta      = request.PriceDelta,
            IsDefault       = request.IsDefault,
            IsActive        = true,
            SortOrder       = request.SortOrder
        };

        db.ModifierOptions.Add(option);
        await db.SaveChangesAsync();

        return (ToOptionDto(option), null);
    }

    public async Task<(ModifierOptionDto? Option, string? Error)> UpdateOptionAsync(
        Guid groupId, Guid optionId, UpdateModifierOptionRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .FirstOrDefaultAsync(g =>
                g.Id == groupId &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (null, "GROUP_NOT_FOUND");

        var option = await db.ModifierOptions
            .FirstOrDefaultAsync(o => o.Id == optionId && o.ModifierGroupId == groupId);

        if (option is null) return (null, "OPTION_NOT_FOUND");

        option.Name       = request.Name.Trim();
        option.PriceDelta = request.PriceDelta;
        option.IsDefault  = request.IsDefault;
        option.SortOrder  = request.SortOrder;
        option.IsActive   = request.IsActive;

        await db.SaveChangesAsync();
        return (ToOptionDto(option), null);
    }

    public async Task<(bool Success, string? Error)> DeactivateOptionAsync(
        Guid groupId, Guid optionId, Guid restaurantId, Guid? branchId = null)
    {
        var group = await db.ModifierGroups
            .FirstOrDefaultAsync(g =>
                g.Id == groupId &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (false, "GROUP_NOT_FOUND");

        var option = await db.ModifierOptions
            .FirstOrDefaultAsync(o => o.Id == optionId && o.ModifierGroupId == groupId);

        if (option is null) return (false, "OPTION_NOT_FOUND");

        option.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    private static ModifierGroupDto ToDto(ModifierGroup g) => new(
        g.Id,
        g.BranchId,
        g.Branch?.Name ?? string.Empty,
        g.Name,
        g.SelectionType.ToString(),
        g.IsRequired,
        g.MinSelections,
        g.MaxSelections,
        g.SortOrder,
        g.IsActive,
        g.CreatedAt,
        g.Options.Where(o => o.IsActive).OrderBy(o => o.SortOrder)
            .Select(o => new ModifierOptionDto(o.Id, o.Name, o.PriceDelta, o.IsDefault, o.IsActive, o.SortOrder))
    );

    private static ModifierOptionDto ToOptionDto(ModifierOption o) =>
        new(o.Id, o.Name, o.PriceDelta, o.IsDefault, o.IsActive, o.SortOrder);
}
