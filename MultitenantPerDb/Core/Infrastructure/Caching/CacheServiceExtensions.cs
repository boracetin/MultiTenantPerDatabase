using Microsoft.Extensions.Options;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure.Caching;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring cache services
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Add cache services to the service collection based on configuration
    /// </summary>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind cache options from configuration
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        // Always add MemoryCache (needed for InMemory provider)
        services.AddMemoryCache();

        // Get cache options to determine provider
        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();

        // Add Redis if configured
        if (cacheOptions.Provider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(cacheOptions.RedisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is required when using Redis cache provider");
            }

            // Add Redis connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(cacheOptions.RedisConnectionString);
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });
        }

        // Register factory
        services.AddSingleton<CacheServiceFactory>();

        // Register ICacheService using factory
        services.AddSingleton<ICacheService>(sp =>
        {
            var factory = sp.GetRequiredService<CacheServiceFactory>();
            return factory.CreateCacheService();
        });

        return services;
    }
}
