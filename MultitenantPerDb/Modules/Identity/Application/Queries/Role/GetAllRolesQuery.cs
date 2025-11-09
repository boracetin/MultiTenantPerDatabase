using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Modules.Identity.Application.Queries.Role;

public record GetAllRolesQuery : IRequest<GetAllRolesResult>;

public record RoleDto(string Id, string Name);

public record GetAllRolesResult(bool Success, IEnumerable<RoleDto> Roles);

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, GetAllRolesResult>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<GetAllRolesQueryHandler> _logger;

    public GetAllRolesQueryHandler(
        RoleManager<IdentityRole> roleManager,
        ILogger<GetAllRolesQueryHandler> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<GetAllRolesResult> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleDto(r.Id, r.Name!))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} roles", roles.Count);
            return new GetAllRolesResult(true, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return new GetAllRolesResult(false, Enumerable.Empty<RoleDto>());
        }
    }
}
