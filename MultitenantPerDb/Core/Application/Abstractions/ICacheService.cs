namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Generic cache service interface
/// Supports multiple cache providers (InMemory, Redis, etc.)
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set value in cache with expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
