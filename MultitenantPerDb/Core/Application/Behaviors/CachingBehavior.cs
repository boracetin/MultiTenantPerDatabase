using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior for caching query results
/// Only caches queries (read operations), commands are excluded
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only cache queries (read operations)
        if (!IsQuery(requestName))
        {
            return await next();
        }

        // Generate cache key based on request type and parameters
        var cacheKey = GenerateCacheKey(request);

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out TResponse cachedResponse))
        {
            _logger.LogInformation("[CACHE HIT] {RequestName} - Cache Key: {CacheKey}", requestName, cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("[CACHE MISS] {RequestName} - Cache Key: {CacheKey}", requestName, cacheKey);

        // Execute handler and get response
        var response = await next();

        // Cache the response
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_defaultCacheDuration)
            .SetPriority(CacheItemPriority.Normal);

        _cache.Set(cacheKey, response, cacheEntryOptions);

        _logger.LogInformation("[CACHE SET] {RequestName} - Cache Key: {CacheKey} - Duration: {Duration}",
            requestName, cacheKey, _defaultCacheDuration);

        return response;
    }

    private static bool IsQuery(string requestName)
    {
        return requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Get", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("List", StringComparison.OrdinalIgnoreCase) ||
               requestName.Contains("Search", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).FullName;
        var requestJson = JsonSerializer.Serialize(request);
        return $"{requestType}:{requestJson}";
    }
}
