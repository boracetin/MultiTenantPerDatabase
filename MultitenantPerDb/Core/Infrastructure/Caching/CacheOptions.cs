namespace MultitenantPerDb.Core.Infrastructure.Caching;

/// <summary>
/// Cache configuration options from appsettings.json
/// </summary>
public class CacheOptions
{
    public const string SectionName = "CacheSettings";

    /// <summary>
    /// Cache provider type: InMemory or Redis
    /// </summary>
    public string Provider { get; set; } = "InMemory";

    /// <summary>
    /// Redis connection string (required if Provider is Redis)
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Enable cache logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}
