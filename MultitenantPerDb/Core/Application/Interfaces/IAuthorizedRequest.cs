namespace MultitenantPerDb.Core.Application.Interfaces;

/// <summary>
/// Marker interface for requests that require authorization
/// Requests implementing this interface will be checked by AuthorizationBehavior
/// </summary>
public interface IAuthorizedRequest
{
    /// <summary>
    /// Gets the required permissions for this request
    /// Example: ["products:write", "products:delete"]
    /// </summary>
    string[]? RequiredPermissions => null;

    /// <summary>
    /// Gets the required roles for this request
    /// Example: ["Admin", "Manager"]
    /// </summary>
    string[]? RequiredRoles => null;

    /// <summary>
    /// Whether this request requires tenant isolation check
    /// Default: true (recommended for multi-tenant security)
    /// </summary>
    bool RequireTenantIsolation => true;
}
