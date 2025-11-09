using Microsoft.Extensions.Options;
using MultitenantPerDb.Core.Application.Abstractions;
using StackExchange.Redis;

namespace MultitenantPerDb.Core.Infrastructure.Caching;

/// <summary>
/// Factory for creating cache service instances based on configuration
/// </summary>
public class CacheServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CacheServiceFactory> _logger;

    public CacheServiceFactory(
        IServiceProvider serviceProvider,
        IOptions<CacheOptions> cacheOptions,
        ILogger<CacheServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public ICacheService CreateCacheService()
    {
        var provider = _cacheOptions.Provider.ToLowerInvariant();

        _logger.LogInformation("Creating cache service with provider: {Provider}", provider);

        return provider switch
        {
            "redis" => CreateRedisCacheService(),
            "inmemory" => CreateInMemoryCacheService(),
            _ => throw new InvalidOperationException($"Unsupported cache provider: {_cacheOptions.Provider}. Supported providers: InMemory, Redis")
        };
    }

    private ICacheService CreateRedisCacheService()
    {
        if (string.IsNullOrWhiteSpace(_cacheOptions.RedisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is required when using Redis cache provider");
        }

        try
        {
            var redis = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();
            var logger = _serviceProvider.GetRequiredService<ILogger<RedisCacheService>>();

            _logger.LogInformation("Redis cache service created successfully");
            return new RedisCacheService(redis, logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Redis cache service. Falling back to InMemory cache.");
            return CreateInMemoryCacheService();
        }
    }

    private ICacheService CreateInMemoryCacheService()
    {
        var memoryCache = _serviceProvider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        var logger = _serviceProvider.GetRequiredService<ILogger<InMemoryCacheService>>();

        _logger.LogInformation("InMemory cache service created successfully");
        return new InMemoryCacheService(memoryCache, logger);
    }
}
