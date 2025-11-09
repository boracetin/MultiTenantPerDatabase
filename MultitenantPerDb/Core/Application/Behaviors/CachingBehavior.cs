using MediatR;
using Microsoft.Extensions.Caching.Memory;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Application.Interfaces;
using System.Text.Json;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior for caching query results
/// Only caches queries implementing ICacheQuery interface
/// Cache is tenant-aware - each tenant has separate cache entries
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);

    public CachingBehavior(
        IMemoryCache cache,
        ICurrentUserService currentUserService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only cache queries that implement ICacheQuery
        if (request is not ICacheQuery cacheQuery)
        {
            return await next();
        }

        // Check if user is authenticated (required for tenant isolation)
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
        {
            _logger.LogDebug("[CACHE SKIP] {RequestName} - No authenticated user or tenant", requestName);
            return await next();
        }

        // Generate tenant-aware cache key
        var cacheKey = GenerateCacheKey(request, tenantId, userId);

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse) && cachedResponse != null)
        {
            _logger.LogInformation("[CACHE HIT] {RequestName} - Tenant: {TenantId} - User: {UserId}", 
                requestName, tenantId, userId);
            return cachedResponse;
        }

        _logger.LogInformation("[CACHE MISS] {RequestName} - Tenant: {TenantId} - User: {UserId}", 
            requestName, tenantId, userId);

        // Execute handler and get response
        var response = await next();

        // Determine cache duration
        var cacheDuration = cacheQuery.CacheDurationMinutes.HasValue
            ? TimeSpan.FromMinutes(cacheQuery.CacheDurationMinutes.Value)
            : _defaultCacheDuration;

        // Cache the response
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(cacheDuration)
            .SetPriority(CacheItemPriority.Normal);

        _cache.Set(cacheKey, response, cacheEntryOptions);

        _logger.LogInformation("[CACHE SET] {RequestName} - Tenant: {TenantId} - Duration: {Duration}",
            requestName, tenantId, cacheDuration);

        return response;
    }

    private static string GenerateCacheKey(TRequest request, string tenantId, string userId)
    {
        var requestType = typeof(TRequest).FullName;
        var requestJson = JsonSerializer.Serialize(request);
        
        // Include tenant and user in cache key for isolation
        return $"Tenant:{tenantId}:User:{userId}:{requestType}:{requestJson}";
    }
}
