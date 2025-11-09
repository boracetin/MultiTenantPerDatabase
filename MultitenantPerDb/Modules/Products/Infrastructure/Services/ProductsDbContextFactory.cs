using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.Products.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.Products.Infrastructure.Services;

/// <summary>
/// Factory for creating tenant-specific ProductsDbContext instances at runtime
/// Uses caching to avoid repeated database lookups for tenant information
/// </summary>
public class ProductsDbContextFactory : CachedTenantDbContextFactory<ProductsDbContext>
{
    public ProductsDbContextFactory(
        ITenantResolver tenantResolver,
        TenancyDbContext tenancyDbContext,
        ICacheService cacheService,
        ILogger<ProductsDbContextFactory> logger)
        : base(tenantResolver, tenancyDbContext, cacheService, logger)
    {
    }

    protected override void ConfigureDbContext(DbContextOptionsBuilder<ProductsDbContext> optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override ProductsDbContext CreateDbContextInstance(DbContextOptions<ProductsDbContext> options)
    {
        return new ProductsDbContext(options);
    }
}
