using System.Security.Claims;
using MultitenantPerDb.Shared.Kernel.Application.Interfaces;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

/// <summary>
/// Service to access current user information from HttpContext
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User?.FindFirst("sub")?.Value 
                          ?? User?.FindFirst("userId")?.Value;

    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value 
                            ?? User?.FindFirst("username")?.Value;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value 
                         ?? User?.FindFirst("email")?.Value;

    public string? TenantId => User?.FindFirst("tenantId")?.Value 
                            ?? User?.FindFirst("tenant_id")?.Value;

    public string[] Roles => User?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToArray() ?? Array.Empty<string>();

    public string[] Permissions => User?.FindAll("permission")
        .Select(c => c.Value)
        .ToArray() ?? Array.Empty<string>();

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
