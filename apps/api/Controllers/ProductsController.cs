using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("products")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class ProductsController(IProductService productService) : ControllerBase
{
    // ─── JWT helpers ───────────────────────────────────────────────────────────

    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    private Guid? CallerBranchId =>
        Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;

    private bool IsOwner => User.IsInRole("Owner");

    private bool IsOwnerOrRestaurantManager => IsOwner || User.IsInRole("RestaurantManager");

    /// <summary>
    /// Branch scope for the current caller:
    ///   Owner / RestaurantManager → null (all branches)
    ///   BranchManager             → their branch only
    /// </summary>
    private Guid? CallerScope => IsOwnerOrRestaurantManager ? null : CallerBranchId;

    // ─── GET /products ─────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var products = await productService.GetProductsAsync(restaurantId.Value, CallerScope);
        return Ok(products);
    }

    // ─── GET /products/{id} ────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var product = await productService.GetProductAsync(id, restaurantId.Value, CallerScope);
        return product is null ? NotFound() : Ok(product);
    }

    // ─── POST /products ────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var effectiveBranchId = IsOwnerOrRestaurantManager ? request.BranchId : CallerBranchId;
        if (effectiveBranchId is null)
            return BadRequest(new { message = "يجب تحديد الفرع" });

        var (product, error) = await productService.CreateProductAsync(
            request, restaurantId.Value, effectiveBranchId.Value);

        return error switch
        {
            "NOT_FOUND"    => NotFound(),
            "INVALID_TYPE" => BadRequest(new { message = "نوع المنتج غير صالح" }),
            _              => CreatedAtAction(nameof(GetProduct), new { id = product!.Id }, product)
        };
    }

    // ─── PUT /products/{id} ────────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (product, error) = await productService.UpdateProductAsync(
            id, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _           => Ok(product)
        };
    }

    // ─── DELETE /products/{id} ─────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await productService.DeactivateProductAsync(
            id, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"             => NotFound(),
            "PRODUCT_HAS_VARIANTS"  => Conflict(new { message = "يتعذّر حذف المنتج لأنه يحتوي على متغيرات نشطة. أوقف المتغيرات أولاً" }),
            _ when success          => NoContent(),
            _                       => StatusCode(500)
        };
    }

    // ─── GET /products/variants (all active variants, for bundle choice picker) ─
    [HttpGet("variants")]
    public async Task<IActionResult> GetAllVariants()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var variants = await productService.GetAllVariantsAsync(restaurantId.Value, CallerScope);
        return Ok(variants);
    }

    // ─── GET /products/{productId}/variants ────────────────────────────────────
    [HttpGet("{productId:guid}/variants")]
    public async Task<IActionResult> GetVariants(Guid productId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var variants = await productService.GetVariantsAsync(productId, restaurantId.Value, CallerScope);
        return Ok(variants);
    }

    // ─── POST /products/{productId}/variants ───────────────────────────────────
    [HttpPost("{productId:guid}/variants")]
    public async Task<IActionResult> CreateVariant(Guid productId, [FromBody] CreateVariantRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (variant, error) = await productService.CreateVariantAsync(
            productId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"    => NotFound(),
            "DUPLICATE_SKU" => Conflict(new { message = "رمز المنتج (SKU) مستخدم بالفعل" }),
            _ => CreatedAtAction(nameof(GetVariants), new { productId }, variant)
        };
    }

    // ─── PUT /products/{productId}/variants/{id} ───────────────────────────────
    [HttpPut("{productId:guid}/variants/{id:guid}")]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid id, [FromBody] UpdateVariantRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (variant, error) = await productService.UpdateVariantAsync(
            productId, id, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _           => Ok(variant)
        };
    }

    // ─── DELETE /products/{productId}/variants/{id} ────────────────────────────
    [HttpDelete("{productId:guid}/variants/{id:guid}")]
    public async Task<IActionResult> DeactivateVariant(Guid productId, Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await productService.DeactivateVariantAsync(
            productId, id, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" or "PRODUCT_NOT_FOUND" or "VARIANT_NOT_FOUND" => NotFound(),
            "VARIANT_IN_USE" => Conflict(new { message = "يتعذّر حذف هذا المتغير لأنه مستخدم في فروع أو وجبات. أزل الاستخدامات أولاً" }),
            _ when success   => NoContent(),
            _                => StatusCode(500)
        };
    }

    // ─── GET /products/{productId}/variants/{variantId}/modifier-groups ────────
    [HttpGet("{productId:guid}/variants/{variantId:guid}/modifier-groups")]
    public async Task<IActionResult> GetVariantModifierGroups(Guid productId, Guid variantId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var groups = await productService.GetVariantModifierGroupsAsync(
            productId, variantId, restaurantId.Value, CallerScope);
        return Ok(groups);
    }

    // ─── POST /products/{productId}/variants/{variantId}/modifier-groups ───────
    [HttpPost("{productId:guid}/variants/{variantId:guid}/modifier-groups")]
    public async Task<IActionResult> LinkModifierGroup(
        Guid productId, Guid variantId, [FromBody] LinkModifierGroupRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (result, error) = await productService.LinkModifierGroupAsync(
            productId, variantId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"      => NotFound(),
            "ALREADY_LINKED" => Conflict(new { message = "مجموعة المعدّلات مرتبطة بالفعل" }),
            "GROUP_NOT_FOUND" => NotFound(),
            _                => Ok(result)
        };
    }

    // ─── DELETE /products/{productId}/variants/{variantId}/modifier-groups/{groupId} ─
    [HttpDelete("{productId:guid}/variants/{variantId:guid}/modifier-groups/{groupId:guid}")]
    public async Task<IActionResult> UnlinkModifierGroup(Guid productId, Guid variantId, Guid groupId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await productService.UnlinkModifierGroupAsync(
            productId, variantId, groupId, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _ when success => NoContent(),
            _ => StatusCode(500)
        };
    }
}
