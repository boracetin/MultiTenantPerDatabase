using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MultitenantPerDb.Modules.Identity.Application.Commands.Role;

public record CreateRoleCommand(string RoleName) : IRequest<CreateRoleResult>;

public record CreateRoleResult(bool Success, string Message, string? RoleId = null);

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, CreateRoleResult>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        RoleManager<IdentityRole> roleManager,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<CreateRoleResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if role already exists
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (roleExists)
            {
                _logger.LogWarning("Role already exists: {RoleName}", request.RoleName);
                return new CreateRoleResult(false, $"Role '{request.RoleName}' already exists");
            }

            // Create new role
            var role = new IdentityRole(request.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role created successfully: {RoleName}", request.RoleName);
                return new CreateRoleResult(true, "Role created successfully", role.Id);
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create role: {Errors}", errors);
            return new CreateRoleResult(false, $"Failed to create role: {errors}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", request.RoleName);
            return new CreateRoleResult(false, "An error occurred while creating the role");
        }
    }
}
