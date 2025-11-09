using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MultitenantPerDb.Modules.Identity.Application.Commands.Role;

public record RemoveRoleFromUserCommand(string UserId, string RoleName) : IRequest<RemoveRoleResult>;

public record RemoveRoleResult(bool Success, string Message);

public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, RemoveRoleResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<RemoveRoleFromUserCommandHandler> _logger;

    public RemoveRoleFromUserCommandHandler(
        UserManager<IdentityUser> userManager,
        ILogger<RemoveRoleFromUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<RemoveRoleResult> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                return new RemoveRoleResult(false, "User not found");
            }

            var userHasRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (!userHasRole)
            {
                _logger.LogWarning("User does not have role: {UserId} - {RoleName}", request.UserId, request.RoleName);
                return new RemoveRoleResult(false, $"User does not have role '{request.RoleName}'");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role removed from user: {UserId} - {RoleName}", request.UserId, request.RoleName);
                return new RemoveRoleResult(true, $"Role '{request.RoleName}' removed successfully");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to remove role: {Errors}", errors);
            return new RemoveRoleResult(false, $"Failed to remove role: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId} - {RoleName}", request.UserId, request.RoleName);
            return new RemoveRoleResult(false, "An error occurred while removing the role");
        }
    }
}
