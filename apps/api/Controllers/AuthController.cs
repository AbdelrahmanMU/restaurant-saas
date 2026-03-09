using Microsoft.AspNetCore.Mvc;
using RestaurantSaas.Api.DTOs.Auth;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (result is null)
            return Unauthorized(new { message = "رقم الهاتف أو كلمة المرور غير صحيحة" });

        return Ok(result);
    }

    [HttpPost("register-owner")]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerRequest request)
    {
        var (result, error) = await authService.RegisterOwnerAsync(request);

        if (error == "PHONE_TAKEN")
            return Conflict(new { message = "رقم الهاتف مسجّل بالفعل" });

        return Ok(result);
    }

    [HttpPost("activate-invite")]
    public async Task<IActionResult> ActivateInvite([FromBody] ActivateInviteRequest request)
    {
        var result = await authService.ActivateInviteAsync(request);

        if (result is null)
            return BadRequest(new { message = "رمز الدعوة غير صالح أو منتهي الصلاحية" });

        return Ok(result);
    }
}
