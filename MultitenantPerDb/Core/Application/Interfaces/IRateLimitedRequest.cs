namespace MultitenantPerDb.Core.Application.Interfaces;

/// <summary>
/// Marker interface for requests that require rate limiting
/// Requests implementing this interface will be checked by RateLimitingBehavior
/// </summary>
public interface IRateLimitedRequest
{
    /// <summary>
    /// Maximum requests allowed in the time window
    /// Default: 100 requests
    /// </summary>
    int Limit => 100;

    /// <summary>
    /// Time window in seconds
    /// Default: 60 seconds (1 minute)
    /// </summary>
    int WindowSeconds => 60;

    /// <summary>
    /// Rate limit scope
    /// PerUser: Limit per user across all tenants
    /// PerTenant: Limit per tenant
    /// PerUserPerTenant: Limit per user in each tenant (most restrictive)
    /// Global: Global limit for this endpoint
    /// </summary>
    RateLimitScope Scope => RateLimitScope.PerUserPerTenant;
}

/// <summary>
/// Rate limiting scope
/// </summary>
public enum RateLimitScope
{
    /// <summary>
    /// Global limit for the endpoint (all users, all tenants)
    /// </summary>
    Global,

    /// <summary>
    /// Limit per tenant (all users in tenant)
    /// </summary>
    PerTenant,

    /// <summary>
    /// Limit per user (across all tenants)
    /// </summary>
    PerUser,

    /// <summary>
    /// Limit per user in each tenant (most restrictive)
    /// </summary>
    PerUserPerTenant
}
