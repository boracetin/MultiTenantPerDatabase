using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Generic factory for creating tenant-specific DbContext instances with caching support
/// Caches tenant information to avoid database lookups on every request
/// Uses Activator.CreateInstance to instantiate DbContext - no need for module-specific factories
/// </summary>
public class ModuleDbContextFactory<TContext> : IModuleDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly ITenantResolver _tenantResolver;
    private readonly IModuleDbContextFactory<TenancyDbContext> _tenancyDbContextFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ModuleDbContextFactory<TContext>> _logger;
    private readonly TimeSpan _tenantCacheDuration = TimeSpan.FromMinutes(30); // Tenant bilgileri nadiren değişir

    public ModuleDbContextFactory(
        ITenantResolver tenantResolver,
        IModuleDbContextFactory<TenancyDbContext> tenancyDbContextFactory,
        ICacheService cacheService,
        ILogger<ModuleDbContextFactory<TContext>> logger)
    {
        _tenantResolver = tenantResolver;
        _tenancyDbContextFactory = tenancyDbContextFactory;
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
            
            // Use factory to create TenancyDbContext on-demand
            using var tenancyDbContext = _tenancyDbContextFactory.CreateDbContext();
            tenant = tenancyDbContext.Tenants.Find(tenantId);

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
        optionsBuilder.UseSqlServer(tenant.ConnectionString);

        // Use Activator to create DbContext instance - works for any DbContext with DbContextOptions<TContext> constructor
        var context = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
        
        _logger.LogDebug("Created {ContextType} for tenant {TenantId}", typeof(TContext).Name, tenantId);
        
        return context;
    }
}
