using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Modules.User.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.User.Infrastructure.Services;

/// <summary>
/// Factory for creating tenant-specific UserDbContext instances at runtime
/// Implements ICanAccessDbContext to allow TenancyDbContext injection for tenant resolution
/// Synchronous factory - tenant resolution and DbContext creation are CPU-bound operations
/// </summary>
public class UserDbContextFactory : ITenantDbContextFactory<UserDbContext>, ICanAccessDbContext
{
    private readonly ITenantResolver _tenantResolver;
    private readonly TenancyDbContext _tenancyDbContext;

    public UserDbContextFactory(ITenantResolver tenantResolver, TenancyDbContext tenancyDbContext)
    {
        _tenantResolver = tenantResolver;
        _tenancyDbContext = tenancyDbContext;
    }

    public UserDbContext CreateDbContext()
    {
        var tenantId = int.Parse(_tenantResolver.TenantId ?? throw new InvalidOperationException("TenantId not found"));
        var tenant = _tenancyDbContext.Tenants.Find(tenantId);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant {tenantId} not found");

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(tenant.ConnectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
