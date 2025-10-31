using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultitenantPerDb.Application.DTOs;
using MultitenantPerDb.Application.Services;
using System.Security.Claims;

namespace MultitenantPerDb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döner (TenantId claim'i içerir)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" });
        }

        return Ok(response);
    }

    /// <summary>
    /// Login olmuş kullanıcının bilgilerini döner (TenantId dahil)
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var tenantId = User.FindFirst("TenantId")?.Value;

        return Ok(new
        {
            userId = userId,
            username = username,
            email = email,
            tenantId = tenantId,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    /// <summary>
    /// Test endpoint - Token geçerliliğini kontrol eder
    /// Authorization: Bearer {token}
    /// </summary>
    [HttpGet("test")]
    [Authorize]
    public ActionResult TestAuth()
    {
        return Ok(new
        {
            message = "Token geçerli!",
            user = User.Identity?.Name,
            tenantId = User.FindFirst("TenantId")?.Value
        });
    }
}
