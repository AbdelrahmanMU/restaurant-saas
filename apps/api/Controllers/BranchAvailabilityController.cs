using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("branch-availability")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class BranchAvailabilityController(IBranchAvailabilityService branchAvailabilityService) : ControllerBase
{
    // ─── JWT helpers ───────────────────────────────────────────────────────────

    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    private Guid? CallerBranchId =>
        Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;

    private bool IsOwner => User.IsInRole("Owner");

    private bool IsOwnerOrRestaurantManager => IsOwner || User.IsInRole("RestaurantManager");

    /// <summary>
    /// Resolves the effective branch ID for this request.
    /// BranchManager: always their JWT branch_id.
    /// Owner/RestaurantManager: must supply ?branchId= query param.
    /// Returns (branchId, isMissing) — isMissing=true means caller must return BadRequest.
    /// </summary>
    private (Guid? BranchId, bool IsMissing) ResolveEffectiveBranchId()
    {
        if (!IsOwnerOrRestaurantManager)
            return (CallerBranchId, CallerBranchId is null);

        var raw = Request.Query["branchId"].FirstOrDefault();
        if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var parsed))
            return (null, true);

        return (parsed, false);
    }

    // ─── GET /branch-availability?branchId=xxx ─────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAvailability()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (effectiveBranchId, isMissing) = ResolveEffectiveBranchId();
        if (isMissing || effectiveBranchId is null)
            return BadRequest(new { message = "يجب تحديد الفرع" });

        var availability = await branchAvailabilityService.GetAvailabilityAsync(
            restaurantId.Value, effectiveBranchId.Value);
        return Ok(availability);
    }

    // ─── PUT /branch-availability/{variantId}?branchId=xxx ────────────────────
    [HttpPut("{variantId:guid}")]
    public async Task<IActionResult> Upsert(Guid variantId, [FromBody] UpsertBranchVariantRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (effectiveBranchId, isMissing) = ResolveEffectiveBranchId();
        if (isMissing || effectiveBranchId is null)
            return BadRequest(new { message = "يجب تحديد الفرع" });

        try
        {
            var result = await branchAvailabilityService.UpsertAsync(
                variantId, request, restaurantId.Value, effectiveBranchId.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
        {
            return NotFound();
        }
    }
}
