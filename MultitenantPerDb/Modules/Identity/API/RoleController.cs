using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultitenantPerDb.Modules.Identity.Application.Commands.Role;
using MultitenantPerDb.Modules.Identity.Application.Queries.Role;
using MultitenantPerDb.Modules.Identity.Application.Commands.DeleteRole;

namespace MultitenantPerDb.Modules.Identity.API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni bir rol oluşturur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateRoleResult>> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Tüm rolleri getirir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetAllRolesResult>> GetAllRoles()
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Belirli bir rolü siler
    /// </summary>
    [HttpDelete("{roleId}")]
    public async Task<ActionResult<DeleteRoleResult>> DeleteRole(string roleId)
    {
        var command = new DeleteRoleCommand(roleId);
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcıya rol atar
    /// </summary>
    [HttpPost("assign")]
    public async Task<ActionResult<AssignRoleResult>> AssignRoleToUser([FromBody] AssignRoleToUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcıdan rol kaldırır
    /// </summary>
    [HttpPost("remove")]
    public async Task<ActionResult<RemoveRoleResult>> RemoveRoleFromUser([FromBody] RemoveRoleFromUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Belirli bir kullanıcının rollerini getirir
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<GetUserRolesResult>> GetUserRoles(string userId)
    {
        var query = new GetUserRolesQuery(userId);
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
