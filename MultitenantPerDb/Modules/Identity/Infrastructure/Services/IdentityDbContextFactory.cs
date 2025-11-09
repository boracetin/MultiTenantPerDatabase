using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Factory for creating tenant-specific ApplicationIdentityDbContext instances at runtime
/// Implements ICanAccessDbContext to allow TenancyDbContext injection for tenant resolution
/// Synchronous factory - tenant resolution and DbContext creation are CPU-bound operations
/// </summary>
public class IdentityDbContextFactory : ITenantDbContextFactory<ApplicationIdentityDbContext>, ICanAccessDbContext
{
    private readonly ITenantResolver _tenantResolver;
    private readonly TenancyDbContext _tenancyDbContext;

    public IdentityDbContextFactory(ITenantResolver tenantResolver, TenancyDbContext tenancyDbContext)
    {
        _tenantResolver = tenantResolver;
        _tenancyDbContext = tenancyDbContext;
    }

    public ApplicationIdentityDbContext CreateDbContext()
    {
        var tenantId = int.Parse(_tenantResolver.TenantId ?? throw new InvalidOperationException("TenantId not found"));
        var tenant = _tenancyDbContext.Tenants.Find(tenantId);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant {tenantId} not found");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationIdentityDbContext>();
        optionsBuilder.UseSqlServer(tenant.ConnectionString);

        return new ApplicationIdentityDbContext(optionsBuilder.Options);
    }
}
