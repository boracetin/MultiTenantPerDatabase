using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure;
using MultitenantPerDb.Modules.User.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.User.Infrastructure.Services;

/// <summary>
/// Factory for creating tenant-specific UserDbContext instances at runtime
/// Uses caching to avoid repeated database lookups for tenant information
/// </summary>
public class UserDbContextFactory : CachedTenantDbContextFactory<UserDbContext>
{
    public UserDbContextFactory(
        ITenantResolver tenantResolver,
        TenancyDbContext tenancyDbContext,
        ICacheService cacheService,
        ILogger<UserDbContextFactory> logger)
        : base(tenantResolver, tenancyDbContext, cacheService, logger)
    {
    }

    protected override void ConfigureDbContext(DbContextOptionsBuilder<UserDbContext> optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override UserDbContext CreateDbContextInstance(DbContextOptions<UserDbContext> options)
    {
        return new UserDbContext(options);
    }
}
