using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class MenuSectionService(AppDbContext db) : IMenuSectionService
{
    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<MenuSectionDto>> GetSectionsAsync(
        Guid restaurantId, Guid? branchId = null)
    {
        var query = db.MenuSections
            .Include(s => s.Branch)
            .Where(s => s.RestaurantId == restaurantId);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => ToDto(s))
            .ToListAsync();
    }

    public async Task<MenuSectionDto?> GetSectionAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var section = await db.MenuSections
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.RestaurantId == restaurantId &&
                (branchId == null || s.BranchId == branchId.Value));

        return section is null ? null : ToDto(section);
    }

    // ─── Mutations ─────────────────────────────────────────────────────────────

    public async Task<(MenuSectionDto? Section, string? Error)> CreateSectionAsync(
        CreateMenuSectionRequest request, Guid restaurantId, Guid branchId)
    {
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId);
        if (branch is null) return (null, "BRANCH_NOT_FOUND");

        var nameTrimmed = request.Name.Trim();
        var duplicate = await db.MenuSections.AnyAsync(s =>
            s.BranchId == branchId && s.Name == nameTrimmed);
        if (duplicate) return (null, "DUPLICATE_NAME");

        var section = new MenuSection
        {
            Id           = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId     = branchId,
            Name         = nameTrimmed,
            Description  = request.Description?.Trim(),
            SortOrder    = request.SortOrder,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            Branch       = branch
        };

        db.MenuSections.Add(section);
        await db.SaveChangesAsync();

        return (ToDto(section), null);
    }

    public async Task<(MenuSectionDto? Section, string? Error)> UpdateSectionAsync(
        Guid id, UpdateMenuSectionRequest request, Guid restaurantId, Guid? branchId = null)
    {
        var section = await db.MenuSections
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.RestaurantId == restaurantId &&
                (branchId == null || s.BranchId == branchId.Value));

        if (section is null) return (null, "NOT_FOUND");

        var nameTrimmed = request.Name.Trim();
        var duplicate = await db.MenuSections.AnyAsync(s =>
            s.BranchId == section.BranchId &&
            s.Name == nameTrimmed &&
            s.Id != id);
        if (duplicate) return (null, "DUPLICATE_NAME");

        section.Name        = nameTrimmed;
        section.Description = request.Description?.Trim();
        section.SortOrder   = request.SortOrder;
        section.IsActive    = request.IsActive;

        await db.SaveChangesAsync();
        return (ToDto(section), null);
    }

    public async Task<(bool Success, string? Error)> DeactivateSectionAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var section = await db.MenuSections
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.RestaurantId == restaurantId &&
                (branchId == null || s.BranchId == branchId.Value));

        if (section is null) return (false, "NOT_FOUND");

        var hasProducts = await db.Products
            .AnyAsync(p => p.MenuSectionId == id && p.IsActive);
        if (hasProducts) return (false, "SECTION_HAS_PRODUCTS");

        section.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Helper ────────────────────────────────────────────────────────────────

    private static MenuSectionDto ToDto(MenuSection s) => new(
        s.Id,
        s.BranchId,
        s.Branch?.Name ?? string.Empty,
        s.Name,
        s.Description,
        s.SortOrder,
        s.IsActive,
        s.CreatedAt
    );
}
