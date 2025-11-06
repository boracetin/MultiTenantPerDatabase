using Microsoft.Extensions.Caching.Memory;
using MultitenantPerDb.Shared.Kernel.Application.Interfaces;
using System.Collections.Concurrent;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

/// <summary>
/// In-memory rate limiting service using sliding window algorithm
/// For distributed systems, replace with Redis-based implementation
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitService> _logger;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public RateLimitService(
        IMemoryCache cache,
        ILogger<RateLimitService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, int limit, int windowSeconds)
    {
        var rateLimitKey = $"ratelimit:{key}";
        var semaphore = _locks.GetOrAdd(rateLimitKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowStart = now - windowSeconds;

            // Get or create request timestamps list
            var requests = _cache.GetOrCreate(rateLimitKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSeconds * 2);
                return new List<long>();
            })!;

            // Remove old requests outside the window
            requests.RemoveAll(timestamp => timestamp < windowStart);

            // Check if limit exceeded
            if (requests.Count >= limit)
            {
                _logger.LogWarning(
                    "[RATE LIMIT EXCEEDED] Key: {Key} - Limit: {Limit}/{WindowSeconds}s - Current: {Current}",
                    key, limit, windowSeconds, requests.Count);
                return false;
            }

            // Add current request
            requests.Add(now);

            // Update cache
            _cache.Set(rateLimitKey, requests, TimeSpan.FromSeconds(windowSeconds * 2));

            _logger.LogDebug(
                "[RATE LIMIT OK] Key: {Key} - Limit: {Limit}/{WindowSeconds}s - Current: {Current}",
                key, limit, windowSeconds, requests.Count);

            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<int> GetRemainingRequestsAsync(string key, int limit, int windowSeconds)
    {
        var rateLimitKey = $"ratelimit:{key}";
        var semaphore = _locks.GetOrAdd(rateLimitKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var windowStart = now - windowSeconds;

            var requests = _cache.Get<List<long>>(rateLimitKey) ?? new List<long>();
            requests.RemoveAll(timestamp => timestamp < windowStart);

            return Math.Max(0, limit - requests.Count);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<int> GetResetTimeAsync(string key, int windowSeconds)
    {
        var rateLimitKey = $"ratelimit:{key}";
        var semaphore = _locks.GetOrAdd(rateLimitKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var requests = _cache.Get<List<long>>(rateLimitKey);

            if (requests == null || requests.Count == 0)
                return 0;

            var oldestRequest = requests.Min();
            var resetTime = (int)(oldestRequest + windowSeconds - now);

            return Math.Max(0, resetTime);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
