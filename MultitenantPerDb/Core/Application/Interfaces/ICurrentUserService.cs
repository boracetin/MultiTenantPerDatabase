namespace MultitenantPerDb.Core.Application.Interfaces;

/// <summary>
/// Service to access current user information from HttpContext
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID from claims
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's username from claims
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets the current user's email from claims
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current tenant ID from claims
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the current tenant name from X-Tenant-Name header
    /// </summary>
    string? TenantName { get; }

    /// <summary>
    /// Gets all roles of the current user
    /// </summary>
    string[] Roles { get; }

    /// <summary>
    /// Gets all permissions of the current user
    /// </summary>
    string[] Permissions { get; }

    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
