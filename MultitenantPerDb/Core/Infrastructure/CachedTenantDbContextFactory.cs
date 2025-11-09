using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Base factory class for creating tenant-specific DbContext instances with caching support
/// Caches tenant information to avoid database lookups on every request
/// </summary>
public abstract class CachedTenantDbContextFactory<TContext> : ITenantDbContextFactory<TContext>, ICanAccessDbContext
    where TContext : DbContext
{
    private readonly ITenantResolver _tenantResolver;
    private readonly TenancyDbContext _tenancyDbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger _logger;
    private readonly TimeSpan _tenantCacheDuration = TimeSpan.FromMinutes(30); // Tenant bilgileri nadiren değişir

    protected CachedTenantDbContextFactory(
        ITenantResolver tenantResolver,
        TenancyDbContext tenancyDbContext,
        ICacheService cacheService,
        ILogger logger)
    {
        _tenantResolver = tenantResolver;
        _tenancyDbContext = tenancyDbContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    public TContext CreateDbContext()
    {
        var tenantId = int.Parse(_tenantResolver.TenantId ?? throw new InvalidOperationException("TenantId not found"));
        
        // Cache key for tenant information
        var cacheKey = $"Tenant:Info:{tenantId}";

        // Try to get tenant from cache
        var tenant = _cacheService.GetAsync<Tenant>(cacheKey).GetAwaiter().GetResult();

        if (tenant == null)
        {
            // Cache miss - load from database
            _logger.LogDebug("Tenant {TenantId} not found in cache, loading from database", tenantId);
            
            tenant = _tenancyDbContext.Tenants.Find(tenantId);

            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant {tenantId} not found");
            }

            // Cache tenant information
            _cacheService.SetAsync(cacheKey, tenant, _tenantCacheDuration).GetAwaiter().GetResult();
            _logger.LogDebug("Cached tenant {TenantId} for {Duration} minutes", tenantId, _tenantCacheDuration.TotalMinutes);
        }
        else
        {
            _logger.LogDebug("Tenant {TenantId} loaded from cache", tenantId);
        }

        // Create DbContext with tenant's connection string
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        ConfigureDbContext(optionsBuilder, tenant.ConnectionString);

        return CreateDbContextInstance(optionsBuilder.Options);
    }

    /// <summary>
    /// Configure DbContext options (e.g., UseSqlServer, UseNpgsql, etc.)
    /// </summary>
    protected abstract void ConfigureDbContext(DbContextOptionsBuilder<TContext> optionsBuilder, string connectionString);

    /// <summary>
    /// Create instance of the specific DbContext
    /// </summary>
    protected abstract TContext CreateDbContextInstance(DbContextOptions<TContext> options);
}
