using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultitenantPerDb.Modules.Main.Identity.Application.Features.Auth.Login;
using MultitenantPerDb.Modules.Main.Identity.Application.Features.Auth.Register;
using MultitenantPerDb.Modules.Main.Identity.Application.DTOs;
using System.Security.Claims;

namespace MultitenantPerDb.Modules.Main.Identity.API;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döner (TenantId claim'i içerir)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var user = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCurrentUser), null, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
