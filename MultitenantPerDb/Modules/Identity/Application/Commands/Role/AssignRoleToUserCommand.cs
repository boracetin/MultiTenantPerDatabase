using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MultitenantPerDb.Modules.Identity.Application.Commands.Role;

public record AssignRoleToUserCommand(string UserId, string RoleName) : IRequest<AssignRoleResult>;

public record AssignRoleResult(bool Success, string Message);

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, AssignRoleResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AssignRoleToUserCommandHandler> _logger;

    public AssignRoleToUserCommandHandler(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AssignRoleToUserCommandHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<AssignRoleResult> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                return new AssignRoleResult(false, "User not found");
            }

            // Check if role exists
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                _logger.LogWarning("Role not found: {RoleName}", request.RoleName);
                return new AssignRoleResult(false, $"Role '{request.RoleName}' not found");
            }

            // Check if user already has the role
            var userHasRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (userHasRole)
            {
                _logger.LogWarning("User already has role: {UserId} - {RoleName}", request.UserId, request.RoleName);
                return new AssignRoleResult(false, $"User already has role '{request.RoleName}'");
            }

            // Assign role to user
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role assigned to user: {UserId} - {RoleName}", request.UserId, request.RoleName);
                return new AssignRoleResult(true, $"Role '{request.RoleName}' assigned successfully");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to assign role: {Errors}", errors);
            return new AssignRoleResult(false, $"Failed to assign role: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId} - {RoleName}", request.UserId, request.RoleName);
            return new AssignRoleResult(false, "An error occurred while assigning the role");
        }
    }
}
