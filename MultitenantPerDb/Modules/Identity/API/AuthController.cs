using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultitenantPerDb.Modules.Identity.Application.Commands.Login;
using MultitenantPerDb.Modules.Identity.Application.Commands.Role;
using MultitenantPerDb.Modules.Identity.Application.Queries.Role;
using MultitenantPerDb.Modules.Identity.Application.Queries.Login;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using System.Security.Claims;

namespace MultitenantPerDb.Modules.Identity.API;

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
    /// Kimlik doğrulaması yapılmış kullanıcının bilgilerini döner
    /// </summary>
    [HttpGet("whoami")]
    [Authorize]
    public async Task<ActionResult<WhoAmIResult>> WhoAmI()
    {
        var query = new WhoAmIQuery();
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
