using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Factory for creating tenant-specific ApplicationIdentityDbContext instances at runtime
/// Uses caching to avoid repeated database lookups for tenant information
/// </summary>
public class IdentityDbContextFactory : CachedTenantDbContextFactory<ApplicationIdentityDbContext>
{
    public IdentityDbContextFactory(
        ITenantResolver tenantResolver,
        TenancyDbContext tenancyDbContext,
        ICacheService cacheService,
        ILogger<IdentityDbContextFactory> logger)
        : base(tenantResolver, tenancyDbContext, cacheService, logger)
    {
    }

    protected override void ConfigureDbContext(DbContextOptionsBuilder<ApplicationIdentityDbContext> optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override ApplicationIdentityDbContext CreateDbContextInstance(DbContextOptions<ApplicationIdentityDbContext> options)
    {
        return new ApplicationIdentityDbContext(options);
    }
}
