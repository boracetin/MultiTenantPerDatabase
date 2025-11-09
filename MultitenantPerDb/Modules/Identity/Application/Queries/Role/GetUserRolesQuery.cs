using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MultitenantPerDb.Modules.Identity.Application.Queries.Role;

public record GetUserRolesQuery(string UserId) : IRequest<GetUserRolesResult>;

public record GetUserRolesResult(bool Success, IEnumerable<string> Roles, string? Message = null);

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, GetUserRolesResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<GetUserRolesQueryHandler> _logger;

    public GetUserRolesQueryHandler(
        UserManager<IdentityUser> userManager,
        ILogger<GetUserRolesQueryHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GetUserRolesResult> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", request.UserId);
                return new GetUserRolesResult(false, Enumerable.Empty<string>(), "User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("Retrieved {Count} roles for user: {UserId}", roles.Count, request.UserId);
            
            return new GetUserRolesResult(true, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user: {UserId}", request.UserId);
            return new GetUserRolesResult(false, Enumerable.Empty<string>(), "An error occurred");
        }
    }
}
