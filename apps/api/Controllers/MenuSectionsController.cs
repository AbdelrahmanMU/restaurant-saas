using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("menu-sections")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class MenuSectionsController(IMenuSectionService menuSectionService) : ControllerBase
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

    // ─── GET /menu-sections ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetSections()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var sections = await menuSectionService.GetSectionsAsync(restaurantId.Value, CallerScope);
        return Ok(sections);
    }

    // ─── GET /menu-sections/{id} ───────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSection(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var section = await menuSectionService.GetSectionAsync(id, restaurantId.Value, CallerScope);
        return section is null ? NotFound() : Ok(section);
    }

    // ─── POST /menu-sections ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateSection([FromBody] CreateMenuSectionRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var effectiveBranchId = IsOwnerOrRestaurantManager ? request.BranchId : CallerBranchId;
        if (effectiveBranchId is null)
            return BadRequest(new { message = "يجب تحديد الفرع" });

        var (section, error) = await menuSectionService.CreateSectionAsync(
            request, restaurantId.Value, effectiveBranchId.Value);

        return error switch
        {
            "NOT_FOUND"      => NotFound(),
            "DUPLICATE_NAME" => Conflict(new { message = "يوجد قسم بهذا الاسم في هذا الفرع" }),
            _                => CreatedAtAction(nameof(GetSection), new { id = section!.Id }, section)
        };
    }

    // ─── PUT /menu-sections/{id} ───────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateMenuSectionRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (section, error) = await menuSectionService.UpdateSectionAsync(
            id, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"      => NotFound(),
            "DUPLICATE_NAME" => Conflict(new { message = "يوجد قسم بهذا الاسم في هذا الفرع" }),
            _                => Ok(section)
        };
    }

    // ─── DELETE /menu-sections/{id} ────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateSection(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await menuSectionService.DeactivateSectionAsync(
            id, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"            => NotFound(),
            "SECTION_HAS_PRODUCTS" => Conflict(new { message = "يتعذّر حذف القسم لأنه يحتوي على منتجات نشطة. أوقف المنتجات أولاً" }),
            _ when success         => NoContent(),
            _                      => StatusCode(500)
        };
    }
}
