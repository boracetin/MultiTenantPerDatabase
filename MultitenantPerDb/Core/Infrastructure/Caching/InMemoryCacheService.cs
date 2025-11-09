using Microsoft.Extensions.Caching.Memory;
using MultitenantPerDb.Core.Application.Abstractions;
using System.Text.Json;

namespace MultitenantPerDb.Core.Infrastructure.Caching;

/// <summary>
/// In-Memory cache implementation
/// Fast but not distributed - data stored in application memory
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache cache, ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from InMemory cache for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                cacheOptions.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Default 5 minutes
            }

            cacheOptions.SetPriority(CacheItemPriority.Normal);

            _cache.Set(key, value, cacheOptions);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in InMemory cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from InMemory cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in InMemory cache for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // IMemoryCache doesn't have a Clear method, we need to track keys separately
            // For now, just log a warning
            _logger.LogWarning("Clear operation is not supported for InMemory cache");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing InMemory cache");
            return Task.CompletedTask;
        }
    }
}
