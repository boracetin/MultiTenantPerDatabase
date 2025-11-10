using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MultitenantPerDb.Modules.Identity.Application.Commands.DeleteRole;

public record DeleteRoleCommand(string RoleId) : IRequest<DeleteRoleResult>;

public record DeleteRoleResult(bool Success, string Message);

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, DeleteRoleResult>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DeleteRoleCommandHandler> _logger;

    public DeleteRoleCommandHandler(
        RoleManager<IdentityRole> roleManager,
        ILogger<DeleteRoleCommandHandler> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<DeleteRoleResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                _logger.LogWarning("Role not found: {RoleId}", request.RoleId);
                return new DeleteRoleResult(false, "Role not found");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role deleted successfully: {RoleName}", role.Name);
                return new DeleteRoleResult(true, "Role deleted successfully");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to delete role: {Errors}", errors);
            return new DeleteRoleResult(false, $"Failed to delete role: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", request.RoleId);
            return new DeleteRoleResult(false, "An error occurred while deleting the role");
        }
    }
}
