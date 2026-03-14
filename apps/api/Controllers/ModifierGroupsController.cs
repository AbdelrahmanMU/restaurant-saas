using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Menu;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("modifier-groups")]
[Authorize(Roles = "Owner,RestaurantManager,BranchManager")]
public class ModifierGroupsController(IModifierGroupService modifierGroupService) : ControllerBase
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

    // ─── GET /modifier-groups ──────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetGroups()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var groups = await modifierGroupService.GetGroupsAsync(restaurantId.Value, CallerScope);
        return Ok(groups);
    }

    // ─── GET /modifier-groups/{id} ─────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var group = await modifierGroupService.GetGroupAsync(id, restaurantId.Value, CallerScope);
        return group is null ? NotFound() : Ok(group);
    }

    // ─── POST /modifier-groups ─────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateModifierGroupRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var effectiveBranchId = IsOwnerOrRestaurantManager ? request.BranchId : CallerBranchId;
        if (effectiveBranchId is null)
            return BadRequest(new { message = "يجب تحديد الفرع" });

        var (group, error) = await modifierGroupService.CreateGroupAsync(
            request, restaurantId.Value, effectiveBranchId.Value);

        return error switch
        {
            "BRANCH_NOT_FOUND"       => NotFound(),
            "INVALID_SELECTION_TYPE" => BadRequest(new { message = "نوع الاختيار غير صالح" }),
            _                        => CreatedAtAction(nameof(GetGroup), new { id = group!.Id }, group)
        };
    }

    // ─── PUT /modifier-groups/{id} ─────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateModifierGroupRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (group, error) = await modifierGroupService.UpdateGroupAsync(
            id, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"              => NotFound(),
            "INVALID_SELECTION_TYPE" => BadRequest(new { message = "نوع الاختيار غير صالح" }),
            _                        => Ok(group)
        };
    }

    // ─── DELETE /modifier-groups/{id} ──────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateGroup(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await modifierGroupService.DeactivateGroupAsync(
            id, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND"         => NotFound(),
            "GROUP_HAS_OPTIONS" => Conflict(new { message = "يتعذّر حذف المجموعة لأنها تحتوي على خيارات نشطة. أوقف الخيارات أولاً" }),
            "GROUP_HAS_LINKS"   => Conflict(new { message = "يتعذّر حذف المجموعة لأنها مرتبطة بمنتجات. أزل الروابط أولاً" }),
            _ when success      => NoContent(),
            _                   => StatusCode(500)
        };
    }

    // ─── POST /modifier-groups/{groupId}/options ───────────────────────────────
    [HttpPost("{groupId:guid}/options")]
    public async Task<IActionResult> AddOption(Guid groupId, [FromBody] CreateModifierOptionRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (option, error) = await modifierGroupService.AddOptionAsync(
            groupId, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _           => Ok(option)
        };
    }

    // ─── PUT /modifier-groups/{groupId}/options/{id} ───────────────────────────
    [HttpPut("{groupId:guid}/options/{id:guid}")]
    public async Task<IActionResult> UpdateOption(
        Guid groupId, Guid id, [FromBody] UpdateModifierOptionRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (option, error) = await modifierGroupService.UpdateOptionAsync(
            groupId, id, request, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _           => Ok(option)
        };
    }

    // ─── DELETE /modifier-groups/{groupId}/options/{id} ────────────────────────
    [HttpDelete("{groupId:guid}/options/{id:guid}")]
    public async Task<IActionResult> DeactivateOption(Guid groupId, Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await modifierGroupService.DeactivateOptionAsync(
            groupId, id, restaurantId.Value, CallerScope);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _ when success => NoContent(),
            _ => StatusCode(500)
        };
    }
}
