using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("products")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class BundlesController(IBundleService bundleService) : ControllerBase
{
    // ─── JWT helpers ───────────────────────────────────────────────────────────

    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    private Guid? CallerBranchId =>
        Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;

    private bool IsOwner => User.IsInRole("Owner");

    private bool IsOwnerOrRestaurantManager => IsOwner || User.IsInRole("RestaurantManager");

    private Guid? CallerScope => IsOwnerOrRestaurantManager ? null : CallerBranchId;

    // ─── GET /products/{productId}/bundle ──────────────────────────────────────
    [HttpGet("{productId:guid}/bundle")]
    public async Task<IActionResult> GetBundle(Guid productId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var bundle = await bundleService.GetBundleAsync(productId, restaurantId.Value, CallerScope);
        return bundle is null ? NotFound() : Ok(bundle);
    }

    // ─── POST /products/{productId}/bundle/ensure ──────────────────────────────
    [HttpPost("{productId:guid}/bundle/ensure")]
    public async Task<IActionResult> EnsureBundle(Guid productId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (bundle, error) = await bundleService.EnsureBundleAsync(productId, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"       => NotFound(new { message = "المنتج غير موجود" }),
            "NOT_BUNDLE_TYPE" => BadRequest(new { message = "المنتج ليس من نوع وجبة" }),
            _                 => Ok(bundle)
        };
    }

    // ─── POST /products/{productId}/bundle/slots ───────────────────────────────
    [HttpPost("{productId:guid}/bundle/slots")]
    public async Task<IActionResult> AddSlot(Guid productId, [FromBody] CreateBundleSlotRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (slot, error) = await bundleService.AddSlotAsync(productId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "BUNDLE_NOT_FOUND" => NotFound(new { message = "الوجبة غير موجودة" }),
            _                  => Ok(slot)
        };
    }

    // ─── PUT /products/{productId}/bundle/slots/{slotId} ──────────────────────
    [HttpPut("{productId:guid}/bundle/slots/{slotId:guid}")]
    public async Task<IActionResult> UpdateSlot(
        Guid productId, Guid slotId, [FromBody] UpdateBundleSlotRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (slot, error) = await bundleService.UpdateSlotAsync(
            productId, slotId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "BUNDLE_NOT_FOUND" or "SLOT_NOT_FOUND" => NotFound(),
            _                                       => Ok(slot)
        };
    }

    // ─── DELETE /products/{productId}/bundle/slots/{slotId} ───────────────────
    [HttpDelete("{productId:guid}/bundle/slots/{slotId:guid}")]
    public async Task<IActionResult> DeleteSlot(Guid productId, Guid slotId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await bundleService.DeleteSlotAsync(
            productId, slotId, restaurantId.Value, CallerScope);

        return error switch
        {
            "BUNDLE_NOT_FOUND" or "SLOT_NOT_FOUND" => NotFound(),
            "SLOT_HAS_CHOICES" => Conflict(new { message = "يتعذّر حذف الخانة لأنها تحتوي على اختيارات. أزل الاختيارات أولاً" }),
            _ when success     => NoContent(),
            _                  => StatusCode(500)
        };
    }

    // ─── POST /products/{productId}/bundle/slots/{slotId}/choices ─────────────
    [HttpPost("{productId:guid}/bundle/slots/{slotId:guid}/choices")]
    public async Task<IActionResult> AddChoice(
        Guid productId, Guid slotId, [FromBody] AddBundleSlotChoiceRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (choice, error) = await bundleService.AddChoiceAsync(
            productId, slotId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "BUNDLE_NOT_FOUND" or "SLOT_NOT_FOUND" => NotFound(),
            "VARIANT_NOT_FOUND"                    => NotFound(new { message = "المتغيّر غير موجود" }),
            "DUPLICATE_CHOICE"                     => Conflict(new { message = "الخيار مضاف بالفعل" }),
            _                                      => Ok(choice)
        };
    }

    // ─── PUT /products/{productId}/bundle/slots/{slotId}/choices/{choiceId} ────
    [HttpPut("{productId:guid}/bundle/slots/{slotId:guid}/choices/{choiceId:guid}")]
    public async Task<IActionResult> UpdateChoice(
        Guid productId, Guid slotId, Guid choiceId,
        [FromBody] UpdateBundleSlotChoiceRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (choice, error) = await bundleService.UpdateChoiceAsync(
            productId, slotId, choiceId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _           => Ok(choice)
        };
    }

    // ─── DELETE /products/{productId}/bundle/slots/{slotId}/choices/{choiceId} ─
    [HttpDelete("{productId:guid}/bundle/slots/{slotId:guid}/choices/{choiceId:guid}")]
    public async Task<IActionResult> RemoveChoice(Guid productId, Guid slotId, Guid choiceId)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await bundleService.RemoveChoiceAsync(
            productId, slotId, choiceId, restaurantId.Value, CallerScope);

        return error switch
        {
            "BUNDLE_NOT_FOUND" or "SLOT_NOT_FOUND" or "CHOICE_NOT_FOUND" => NotFound(),
            _ when success => NoContent(),
            _              => StatusCode(500)
        };
    }
}
