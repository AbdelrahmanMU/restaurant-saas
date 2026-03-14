using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class BranchAvailabilityService(AppDbContext db) : IBranchAvailabilityService
{
    public async Task<IEnumerable<BranchVariantDto>> GetAvailabilityAsync(
        Guid restaurantId, Guid branchId)
    {
        var variants = await db.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p.MenuSection)
            .Include(v => v.BranchAvailability.Where(b => b.BranchId == branchId))
            .Where(v =>
                v.Product.RestaurantId == restaurantId &&
                v.Product.BranchId == branchId &&
                v.IsActive &&
                v.Product.IsActive)
            .OrderBy(v => v.Product.Name)
            .ThenBy(v => v.SortOrder)
            .ToListAsync();

        return variants.Select(v =>
        {
            var bpv = v.BranchAvailability.FirstOrDefault();
            return new BranchVariantDto(
                bpv?.Id ?? Guid.Empty,
                branchId,
                v.Id,
                v.Product.Name,
                v.Name,
                v.Product.MenuSection?.Name,
                v.Price,
                bpv?.IsAvailable ?? true,
                bpv?.PriceOverride
            );
        });
    }

    public async Task<BranchVariantDto> UpsertAsync(
        Guid productVariantId, UpsertBranchVariantRequest request,
        Guid restaurantId, Guid branchId)
    {
        var variant = await db.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p.MenuSection)
            .FirstOrDefaultAsync(v =>
                v.Id == productVariantId &&
                v.Product.RestaurantId == restaurantId &&
                v.Product.BranchId == branchId);

        if (variant is null)
            throw new InvalidOperationException("NOT_FOUND");

        var bpv = await db.BranchProductVariants
            .FirstOrDefaultAsync(b =>
                b.BranchId == branchId && b.ProductVariantId == productVariantId);

        if (bpv is null)
        {
            bpv = new BranchProductVariant
            {
                Id               = Guid.NewGuid(),
                BranchId         = branchId,
                ProductVariantId = productVariantId
            };
            db.BranchProductVariants.Add(bpv);
        }

        bpv.IsAvailable   = request.IsAvailable;
        bpv.PriceOverride = request.PriceOverride;

        await db.SaveChangesAsync();

        return new BranchVariantDto(
            bpv.Id,
            branchId,
            variant.Id,
            variant.Product.Name,
            variant.Name,
            variant.Product.MenuSection?.Name,
            variant.Price,
            bpv.IsAvailable,
            bpv.PriceOverride
        );
    }
}
