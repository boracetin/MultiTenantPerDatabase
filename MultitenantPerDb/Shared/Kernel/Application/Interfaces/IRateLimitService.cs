namespace MultitenantPerDb.Shared.Kernel.Application.Interfaces;

/// <summary>
/// Service for rate limiting enforcement
/// Uses in-memory cache (can be replaced with Redis for distributed systems)
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if the request is allowed based on rate limit
    /// </summary>
    /// <param name="key">Unique key for rate limiting (e.g., "userId:tenantId:endpoint")</param>
    /// <param name="limit">Maximum requests allowed</param>
    /// <param name="windowSeconds">Time window in seconds</param>
    /// <returns>True if allowed, false if rate limit exceeded</returns>
    Task<bool> IsAllowedAsync(string key, int limit, int windowSeconds);

    /// <summary>
    /// Gets the remaining requests for a key
    /// </summary>
    Task<int> GetRemainingRequestsAsync(string key, int limit, int windowSeconds);

    /// <summary>
    /// Gets the time until rate limit reset (in seconds)
    /// </summary>
    Task<int> GetResetTimeAsync(string key, int windowSeconds);
}
