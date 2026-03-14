using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Branches;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("branches")]
[Authorize]
public class BranchesController(IBranchService branchService) : ControllerBase
{
    private Guid? RestaurantId =>
        Guid.TryParse(User.FindFirstValue("restaurant_id"), out var id) ? id : null;

    private Guid? CallerBranchId =>
        Guid.TryParse(User.FindFirstValue("branch_id"), out var id) ? id : null;

    private bool IsOwner => User.IsInRole("Owner");

    // GET /branches
    // Owner → all restaurant branches; BranchManager → only their own branch
    [HttpGet]
    public async Task<IActionResult> GetBranches()
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        // BranchManagers are scoped to their branch; all other management roles see everything
        Guid? scope = (!IsOwner && !User.IsInRole("RestaurantManager"))
            ? CallerBranchId
            : null;

        var branches = await branchService.GetBranchesAsync(restaurantId.Value, scope);
        return Ok(branches);
    }

    // GET /branches/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBranch(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        // BranchManager may only access their own branch
        if (!IsOwner && !User.IsInRole("RestaurantManager") && CallerBranchId != id)
            return Forbid();

        var branch = await branchService.GetBranchAsync(id, restaurantId.Value);
        return branch is null ? NotFound() : Ok(branch);
    }

    // POST /branches  — Owner only
    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var branch = await branchService.CreateBranchAsync(request, restaurantId.Value);
        return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, branch);
    }

    // PUT /branches/{id}  — Owner only
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (branch, error) = await branchService.UpdateBranchAsync(id, request, restaurantId.Value);

        return error switch
        {
            "NOT_FOUND" => NotFound(),
            _ => Ok(branch)
        };
    }

    // POST /branches/{id}/manager  — Owner only
    [HttpPost("{id:guid}/manager")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AssignManager(Guid id, [FromBody] AssignManagerRequest request)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (branch, error) = await branchService.AssignManagerAsync(id, request.UserId, restaurantId.Value);

        return error switch
        {
            "BRANCH_NOT_FOUND" => NotFound(new { message = "الفرع غير موجود" }),
            "USER_NOT_FOUND"   => BadRequest(new { message = "المستخدم غير موجود" }),
            _                  => Ok(branch)
        };
    }

    // DELETE /branches/{id}/manager  — Owner only
    [HttpDelete("{id:guid}/manager")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> RemoveManager(Guid id)
    {
        var restaurantId = RestaurantId;
        if (restaurantId is null) return Forbid();

        var (success, error) = await branchService.RemoveManagerAsync(id, restaurantId.Value);

        return error switch
        {
            "BRANCH_NOT_FOUND" => NotFound(new { message = "الفرع غير موجود" }),
            "NO_MANAGER"       => BadRequest(new { message = "لا يوجد مدير معيّن لهذا الفرع" }),
            _ when success     => NoContent(),
            _                  => StatusCode(500)
        };
    }
}
