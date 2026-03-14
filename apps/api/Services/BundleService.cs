using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class BundleService(AppDbContext db) : IBundleService
{
    // ─── Queries ───────────────────────────────────────────────────────────────

    public async Task<BundleDto?> GetBundleAsync(
        Guid productId, Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await db.Bundles
            .Include(b => b.Product)
            .Include(b => b.Slots.Where(s => s.IsActive))
                .ThenInclude(s => s.Choices)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(b =>
                b.ProductId == productId &&
                b.Product.RestaurantId == restaurantId &&
                (branchId == null || b.Product.BranchId == branchId.Value));

        return bundle is null ? null : ToDto(bundle);
    }

    // ─── Ensure / Bootstrap ────────────────────────────────────────────────────

    public async Task<(BundleDto? Bundle, string? Error)> EnsureBundleAsync(
        Guid productId, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (null, "NOT_FOUND");
        if (product.Type != ProductType.Bundle) return (null, "NOT_BUNDLE_TYPE");

        var existing = await db.Bundles
            .Include(b => b.Product)
            .Include(b => b.Slots.Where(s => s.IsActive))
                .ThenInclude(s => s.Choices)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(b => b.ProductId == productId);

        if (existing is not null) return (ToDto(existing), null);

        var bundle = new Bundle
        {
            Id        = Guid.NewGuid(),
            ProductId = productId,
            Slots     = []
        };

        db.Bundles.Add(bundle);
        await db.SaveChangesAsync();

        // Re-fetch with full includes for the response
        var created = await db.Bundles
            .Include(b => b.Product)
            .Include(b => b.Slots.Where(s => s.IsActive))
                .ThenInclude(s => s.Choices)
                    .ThenInclude(c => c.ProductVariant)
                        .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id);

        return (ToDto(created!), null);
    }

    // ─── Slots ─────────────────────────────────────────────────────────────────

    public async Task<(BundleSlotDto? Slot, string? Error)> AddSlotAsync(
        Guid productId, CreateBundleSlotRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (null, "BUNDLE_NOT_FOUND");

        var slot = new BundleSlot
        {
            Id         = Guid.NewGuid(),
            BundleId   = bundle.Id,
            Name       = request.Name.Trim(),
            IsRequired = request.IsRequired,
            MinChoices = request.MinChoices,
            MaxChoices = request.MaxChoices,
            SortOrder  = request.SortOrder,
            Choices    = []
        };

        db.BundleSlots.Add(slot);
        await db.SaveChangesAsync();

        return (ToSlotDto(slot), null);
    }

    public async Task<(BundleSlotDto? Slot, string? Error)> UpdateSlotAsync(
        Guid productId, Guid slotId, UpdateBundleSlotRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (null, "BUNDLE_NOT_FOUND");

        var slot = await db.BundleSlots
            .Include(s => s.Choices)
                .ThenInclude(c => c.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(s => s.Id == slotId && s.BundleId == bundle.Id);

        if (slot is null) return (null, "SLOT_NOT_FOUND");

        slot.Name       = request.Name.Trim();
        slot.IsRequired = request.IsRequired;
        slot.MinChoices = request.MinChoices;
        slot.MaxChoices = request.MaxChoices;
        slot.SortOrder  = request.SortOrder;

        await db.SaveChangesAsync();
        return (ToSlotDto(slot), null);
    }

    public async Task<(bool Success, string? Error)> DeleteSlotAsync(
        Guid productId, Guid slotId, Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (false, "BUNDLE_NOT_FOUND");

        var slot = await db.BundleSlots
            .FirstOrDefaultAsync(s => s.Id == slotId && s.BundleId == bundle.Id);

        if (slot is null) return (false, "SLOT_NOT_FOUND");

        var hasChoices = await db.BundleSlotChoices
            .AnyAsync(c => c.BundleSlotId == slotId);
        if (hasChoices) return (false, "SLOT_HAS_CHOICES");

        slot.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Choices ───────────────────────────────────────────────────────────────

    public async Task<(BundleSlotChoiceDto? Choice, string? Error)> AddChoiceAsync(
        Guid productId, Guid slotId, AddBundleSlotChoiceRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (null, "BUNDLE_NOT_FOUND");

        var slot = await db.BundleSlots
            .FirstOrDefaultAsync(s => s.Id == slotId && s.BundleId == bundle.Id);

        if (slot is null) return (null, "SLOT_NOT_FOUND");

        var duplicate = await db.BundleSlotChoices.AnyAsync(c =>
            c.BundleSlotId == slotId && c.ProductVariantId == request.ProductVariantId);
        if (duplicate) return (null, "DUPLICATE_CHOICE");

        var variant = await db.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

        if (variant is null) return (null, "VARIANT_NOT_FOUND");

        var choice = new BundleSlotChoice
        {
            Id               = Guid.NewGuid(),
            BundleSlotId     = slotId,
            ProductVariantId = request.ProductVariantId,
            PriceDelta       = request.PriceDelta,
            ProductVariant   = variant
        };

        db.BundleSlotChoices.Add(choice);
        await db.SaveChangesAsync();

        return (ToChoiceDto(choice), null);
    }

    public async Task<(BundleSlotChoiceDto? Choice, string? Error)> UpdateChoiceAsync(
        Guid productId, Guid slotId, Guid choiceId,
        UpdateBundleSlotChoiceRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (null, "NOT_FOUND");

        var slot = await db.BundleSlots
            .FirstOrDefaultAsync(s => s.Id == slotId && s.BundleId == bundle.Id);
        if (slot is null) return (null, "NOT_FOUND");

        var choice = await db.BundleSlotChoices
            .Include(c => c.ProductVariant)
                .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.Id == choiceId && c.BundleSlotId == slotId);
        if (choice is null) return (null, "NOT_FOUND");

        choice.PriceDelta = request.PriceDelta;
        await db.SaveChangesAsync();

        return (ToChoiceDto(choice), null);
    }

    public async Task<(bool Success, string? Error)> RemoveChoiceAsync(
        Guid productId, Guid slotId, Guid choiceId,
        Guid restaurantId, Guid? branchId = null)
    {
        var bundle = await FindBundleAsync(productId, restaurantId, branchId);
        if (bundle is null) return (false, "BUNDLE_NOT_FOUND");

        var slot = await db.BundleSlots
            .FirstOrDefaultAsync(s => s.Id == slotId && s.BundleId == bundle.Id);

        if (slot is null) return (false, "SLOT_NOT_FOUND");

        var choice = await db.BundleSlotChoices
            .FirstOrDefaultAsync(c => c.Id == choiceId && c.BundleSlotId == slotId);

        if (choice is null) return (false, "CHOICE_NOT_FOUND");

        db.BundleSlotChoices.Remove(choice);
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private async Task<Bundle?> FindBundleAsync(
        Guid productId, Guid restaurantId, Guid? branchId)
    {
        return await db.Bundles
            .Include(b => b.Product)
            .FirstOrDefaultAsync(b =>
                b.ProductId == productId &&
                b.Product.RestaurantId == restaurantId &&
                (branchId == null || b.Product.BranchId == branchId.Value));
    }

    private static BundleDto ToDto(Bundle b) => new(
        b.Id,
        b.ProductId,
        b.Slots.OrderBy(s => s.SortOrder).Select(s => ToSlotDto(s))
    );

    private static BundleSlotDto ToSlotDto(BundleSlot s) => new(
        s.Id,
        s.Name,
        s.IsRequired,
        s.MinChoices,
        s.MaxChoices,
        s.SortOrder,
        s.Choices.Select(c => ToChoiceDto(c))
    );

    private static BundleSlotChoiceDto ToChoiceDto(BundleSlotChoice c) => new(
        c.Id,
        c.ProductVariantId,
        c.ProductVariant?.Product?.Name ?? string.Empty,
        c.ProductVariant?.Name ?? string.Empty,
        c.ProductVariant?.Price ?? 0,
        c.PriceDelta
    );
}
