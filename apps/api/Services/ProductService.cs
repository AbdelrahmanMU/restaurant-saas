using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class ProductService(AppDbContext db) : IProductService
{
    // ─── Products ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProductSummaryDto>> GetProductsAsync(
        Guid restaurantId, Guid? branchId = null)
    {
        var q = db.Products
            .Include(p => p.Branch)
            .Include(p => p.MenuSection)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Where(p => p.RestaurantId == restaurantId);

        if (branchId.HasValue)
            q = q.Where(p => p.BranchId == branchId.Value);

        var products = await q
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return products.Select(p => ToSummaryDto(p));
    }

    public async Task<ProductDetailDto?> GetProductAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .Include(p => p.Branch)
            .Include(p => p.MenuSection)
            .Include(p => p.Variants.Where(v => v.IsActive))
                .ThenInclude(v => v.ModifierGroups)
                    .ThenInclude(l => l.ModifierGroup)
                        .ThenInclude(g => g.Options)
            .Include(p => p.Variants.Where(v => v.IsActive))
                .ThenInclude(v => v.ModifierGroups)
                    .ThenInclude(l => l.ModifierGroup)
                        .ThenInclude(g => g.Branch)
            .Include(p => p.Bundle)
                .ThenInclude(b => b!.Slots.Where(s => s.IsActive))
                    .ThenInclude(s => s.Choices)
                        .ThenInclude(c => c.ProductVariant)
                            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        return product is null ? null : ToDetailDto(product);
    }

    public async Task<(ProductDetailDto? Product, string? Error)> CreateProductAsync(
        CreateProductRequest request, Guid restaurantId, Guid branchId)
    {
        if (!Enum.TryParse<ProductType>(request.Type, out var productType))
            return (null, "INVALID_PRODUCT_TYPE");

        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId && b.RestaurantId == restaurantId);
        if (branch is null) return (null, "BRANCH_NOT_FOUND");

        var product = new Product
        {
            Id            = Guid.NewGuid(),
            RestaurantId  = restaurantId,
            BranchId      = branchId,
            MenuSectionId = request.MenuSectionId,
            Name          = request.Name.Trim(),
            Description   = request.Description?.Trim(),
            ImageUrl      = request.ImageUrl?.Trim(),
            Type          = productType,
            SortOrder     = request.SortOrder,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow,
            Branch        = branch
        };

        db.Products.Add(product);

        if (productType == ProductType.Bundle)
        {
            var bundle = new Bundle
            {
                Id        = Guid.NewGuid(),
                ProductId = product.Id,
                Slots     = []
            };
            db.Bundles.Add(bundle);
            product.Bundle = bundle;
        }

        await db.SaveChangesAsync();

        // Re-fetch with full detail includes
        var created = await GetProductAsync(product.Id, restaurantId, branchId);
        return (created, null);
    }

    public async Task<(ProductDetailDto? Product, string? Error)> UpdateProductAsync(
        Guid id, UpdateProductRequest request, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .Include(p => p.Bundle)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (null, "NOT_FOUND");

        product.MenuSectionId = request.MenuSectionId;
        product.Name          = request.Name.Trim();
        product.Description   = request.Description?.Trim();
        product.ImageUrl      = request.ImageUrl?.Trim();
        product.SortOrder     = request.SortOrder;
        product.IsActive      = request.IsActive;

        if (product.Type == ProductType.Bundle && product.Bundle is null)
        {
            var bundle = new Bundle
            {
                Id        = Guid.NewGuid(),
                ProductId = product.Id,
                Slots     = []
            };
            db.Bundles.Add(bundle);
        }

        await db.SaveChangesAsync();

        var updated = await GetProductAsync(id, restaurantId, branchId);
        return (updated, null);
    }

    public async Task<(bool Success, string? Error)> DeactivateProductAsync(
        Guid id, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (false, "NOT_FOUND");

        var hasVariants = await db.ProductVariants
            .AnyAsync(v => v.ProductId == id && v.IsActive);
        if (hasVariants) return (false, "PRODUCT_HAS_VARIANTS");

        product.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Variants ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<ProductVariantSummaryDto>> GetAllVariantsAsync(
        Guid restaurantId, Guid? branchId = null)
    {
        var q = db.ProductVariants
            .Include(v => v.Product)
            .Where(v =>
                v.IsActive &&
                v.Product.IsActive &&
                v.Product.RestaurantId == restaurantId);

        if (branchId.HasValue)
            q = q.Where(v => v.Product.BranchId == branchId.Value);

        var variants = await q
            .OrderBy(v => v.Product.SortOrder)
            .ThenBy(v => v.Product.Name)
            .ThenBy(v => v.SortOrder)
            .ToListAsync();

        return variants.Select(v => new ProductVariantSummaryDto(
            v.Id,
            v.ProductId,
            v.Product.Name,
            v.Name,
            v.Price
        ));
    }

    public async Task<IEnumerable<VariantDto>> GetVariantsAsync(
        Guid productId, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .Include(p => p.Variants.Where(v => v.IsActive))
                .ThenInclude(v => v.ModifierGroups)
                    .ThenInclude(l => l.ModifierGroup)
                        .ThenInclude(g => g.Options)
            .Include(p => p.Variants.Where(v => v.IsActive))
                .ThenInclude(v => v.ModifierGroups)
                    .ThenInclude(l => l.ModifierGroup)
                        .ThenInclude(g => g.Branch)
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return [];

        return product.Variants
            .Where(v => v.IsActive)
            .OrderBy(v => v.SortOrder)
            .Select(v => ToVariantDto(v));
    }

    public async Task<(VariantDto? Variant, string? Error)> CreateVariantAsync(
        Guid productId, CreateVariantRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .Include(p => p.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (null, "PRODUCT_NOT_FOUND");

        var hasActiveDefault = product.Variants.Any(v => v.IsActive && v.IsDefault);

        var variant = new ProductVariant
        {
            Id         = Guid.NewGuid(),
            ProductId  = productId,
            Name       = request.Name.Trim(),
            Sku        = request.Sku?.Trim(),
            Price      = request.Price,
            IsDefault  = request.IsDefault || !hasActiveDefault,
            IsActive   = true,
            SortOrder  = request.SortOrder,
            CreatedAt  = DateTime.UtcNow,
            ModifierGroups = []
        };

        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync();

        // Re-fetch with modifier group includes for full DTO
        var created = await db.ProductVariants
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Options)
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Branch)
            .FirstOrDefaultAsync(v => v.Id == variant.Id);

        return (ToVariantDto(created!), null);
    }

    public async Task<(VariantDto? Variant, string? Error)> UpdateVariantAsync(
        Guid productId, Guid variantId, UpdateVariantRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (null, "PRODUCT_NOT_FOUND");

        var variant = await db.ProductVariants
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Options)
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Branch)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

        if (variant is null) return (null, "VARIANT_NOT_FOUND");

        variant.Name      = request.Name.Trim();
        variant.Sku       = request.Sku?.Trim();
        variant.Price     = request.Price;
        variant.IsDefault = request.IsDefault;
        variant.IsActive  = request.IsActive;
        variant.SortOrder = request.SortOrder;

        await db.SaveChangesAsync();
        return (ToVariantDto(variant), null);
    }

    public async Task<(bool Success, string? Error)> DeactivateVariantAsync(
        Guid productId, Guid variantId, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (false, "PRODUCT_NOT_FOUND");

        var variant = await db.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

        if (variant is null) return (false, "VARIANT_NOT_FOUND");

        var inBranchVariants = await db.BranchProductVariants
            .AnyAsync(bpv => bpv.ProductVariantId == variantId);
        var inBundleChoices = await db.BundleSlotChoices
            .AnyAsync(c => c.ProductVariantId == variantId);
        if (inBranchVariants || inBundleChoices) return (false, "VARIANT_IN_USE");

        variant.IsActive = false;
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Modifier Group Links ──────────────────────────────────────────────────

    public async Task<IEnumerable<LinkedModifierGroupDto>> GetVariantModifierGroupsAsync(
        Guid productId, Guid variantId, Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return [];

        var variant = await db.ProductVariants
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Options)
            .Include(v => v.ModifierGroups)
                .ThenInclude(l => l.ModifierGroup)
                    .ThenInclude(g => g.Branch)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

        if (variant is null) return [];

        return variant.ModifierGroups
            .OrderBy(l => l.SortOrder)
            .Select(l => ToLinkedGroupDto(l));
    }

    public async Task<(LinkedModifierGroupDto? Link, string? Error)> LinkModifierGroupAsync(
        Guid productId, Guid variantId, LinkModifierGroupRequest request,
        Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (null, "PRODUCT_NOT_FOUND");

        var variant = await db.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

        if (variant is null) return (null, "VARIANT_NOT_FOUND");

        var group = await db.ModifierGroups
            .FirstOrDefaultAsync(g =>
                g.Id == request.ModifierGroupId &&
                g.RestaurantId == restaurantId &&
                (branchId == null || g.BranchId == branchId.Value));

        if (group is null) return (null, "GROUP_NOT_FOUND");

        var alreadyLinked = await db.ProductVariantModifierGroups.AnyAsync(l =>
            l.ProductVariantId == variantId && l.ModifierGroupId == request.ModifierGroupId);
        if (alreadyLinked) return (null, "ALREADY_LINKED");

        var link = new ProductVariantModifierGroup
        {
            Id               = Guid.NewGuid(),
            ProductVariantId = variantId,
            ModifierGroupId  = request.ModifierGroupId,
            SortOrder        = request.SortOrder
        };

        db.ProductVariantModifierGroups.Add(link);
        await db.SaveChangesAsync();

        // Re-fetch with full includes for response
        var created = await db.ProductVariantModifierGroups
            .Include(l => l.ModifierGroup)
                .ThenInclude(g => g.Options)
            .Include(l => l.ModifierGroup)
                .ThenInclude(g => g.Branch)
            .FirstOrDefaultAsync(l => l.Id == link.Id);

        return (ToLinkedGroupDto(created!), null);
    }

    public async Task<(bool Success, string? Error)> UnlinkModifierGroupAsync(
        Guid productId, Guid variantId, Guid groupId,
        Guid restaurantId, Guid? branchId = null)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.RestaurantId == restaurantId &&
                (branchId == null || p.BranchId == branchId.Value));

        if (product is null) return (false, "PRODUCT_NOT_FOUND");

        var variant = await db.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);

        if (variant is null) return (false, "VARIANT_NOT_FOUND");

        var link = await db.ProductVariantModifierGroups
            .FirstOrDefaultAsync(l =>
                l.ProductVariantId == variantId && l.ModifierGroupId == groupId);

        if (link is null) return (false, "LINK_NOT_FOUND");

        db.ProductVariantModifierGroups.Remove(link);
        await db.SaveChangesAsync();
        return (true, null);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    private static ProductSummaryDto ToSummaryDto(Product p)
    {
        var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
        return new ProductSummaryDto(
            p.Id,
            p.BranchId,
            p.Branch?.Name ?? string.Empty,
            p.MenuSectionId,
            p.MenuSection?.Name,
            p.Name,
            p.Description,
            p.ImageUrl,
            p.Type.ToString(),
            p.SortOrder,
            p.IsActive,
            activeVariants.OrderBy(v => v.Price).FirstOrDefault()?.Price,
            activeVariants.Count,
            p.CreatedAt
        );
    }

    private static ProductDetailDto ToDetailDto(Product p) => new(
        p.Id,
        p.BranchId,
        p.Branch?.Name ?? string.Empty,
        p.MenuSectionId,
        p.MenuSection?.Name,
        p.Name,
        p.Description,
        p.ImageUrl,
        p.Type.ToString(),
        p.SortOrder,
        p.IsActive,
        p.CreatedAt,
        p.Variants
            .Where(v => v.IsActive)
            .OrderBy(v => v.SortOrder)
            .Select(v => ToVariantDto(v)),
        p.Bundle is null ? null : ToBundleDto(p.Bundle)
    );

    private static VariantDto ToVariantDto(ProductVariant v) => new(
        v.Id,
        v.Name,
        v.Sku,
        v.Price,
        v.IsDefault,
        v.IsActive,
        v.SortOrder,
        v.ModifierGroups.OrderBy(l => l.SortOrder).Select(l => ToLinkedGroupDto(l))
    );

    private static LinkedModifierGroupDto ToLinkedGroupDto(ProductVariantModifierGroup l) => new(
        l.Id,
        l.ModifierGroupId,
        l.ModifierGroup?.Name ?? string.Empty,
        l.ModifierGroup?.SelectionType.ToString() ?? string.Empty,
        l.ModifierGroup?.IsRequired ?? false,
        l.ModifierGroup?.MinSelections ?? 0,
        l.ModifierGroup?.MaxSelections,
        l.SortOrder,
        l.ModifierGroup?.Options.Where(o => o.IsActive).OrderBy(o => o.SortOrder)
            .Select(o => new ModifierOptionDto(o.Id, o.Name, o.PriceDelta, o.IsDefault, o.IsActive, o.SortOrder))
        ?? []
    );

    private static BundleDto ToBundleDto(Bundle b) => new(
        b.Id,
        b.ProductId,
        b.Slots.OrderBy(s => s.SortOrder).Select(s => new BundleSlotDto(
            s.Id,
            s.Name,
            s.IsRequired,
            s.MinChoices,
            s.MaxChoices,
            s.SortOrder,
            s.Choices.Select(c => new BundleSlotChoiceDto(
                c.Id,
                c.ProductVariantId,
                c.ProductVariant?.Product?.Name ?? string.Empty,
                c.ProductVariant?.Name ?? string.Empty,
                c.ProductVariant?.Price ?? 0,
                c.PriceDelta
            ))
        ))
    );
}
