using MediatR;
using Microsoft.AspNetCore.Identity;
using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Modules.Identity.Application.Queries.Login;

public record WhoAmIQuery : IRequest<WhoAmIResult>;

public record WhoAmIResult(
    bool Success, 
    string? UserId, 
    string? Username, 
    string? Email,
    string? TenantId,
    string? TenantName,
    IEnumerable<string> Roles,
    string? Message = null);

public class WhoAmIQueryHandler : IRequestHandler<WhoAmIQuery, WhoAmIResult>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<WhoAmIQueryHandler> _logger;

    public WhoAmIQueryHandler(
        ICurrentUserService currentUserService,
        UserManager<IdentityUser> userManager,
        ILogger<WhoAmIQueryHandler> logger)
    {
        _currentUserService = currentUserService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<WhoAmIResult> Handle(WhoAmIQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var tenantId = _currentUserService.TenantId;
            var tenantName = _currentUserService.TenantName;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("WhoAmI query called but no user is authenticated");
                return new WhoAmIResult(
                    false, 
                    null, 
                    null, 
                    null, 
                    null, 
                    null, 
                    Enumerable.Empty<string>(),
                    "User not authenticated");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return new WhoAmIResult(
                    false, 
                    userId, 
                    null, 
                    null, 
                    tenantId, 
                    tenantName, 
                    Enumerable.Empty<string>(),
                    "User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            _logger.LogInformation("WhoAmI query executed for user: {UserId}", userId);
            
            return new WhoAmIResult(
                true,
                user.Id,
                user.UserName,
                user.Email,
                tenantId,
                tenantName,
                roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing WhoAmI query");
            return new WhoAmIResult(
                false, 
                null, 
                null, 
                null, 
                null, 
                null, 
                Enumerable.Empty<string>(),
                "An error occurred");
        }
    }
}
