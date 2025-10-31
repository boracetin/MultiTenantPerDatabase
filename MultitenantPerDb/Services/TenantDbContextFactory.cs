using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Data;

namespace MultitenantPerDb.Services;

public interface ITenantDbContextFactory
{
    Task<ApplicationDbContext> CreateDbContextAsync();
}

public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly ITenantResolver _tenantResolver;
    private readonly TenantDbContext _tenantDbContext;

    public TenantDbContextFactory(ITenantResolver tenantResolver, TenantDbContext tenantDbContext)
    {
        _tenantResolver = tenantResolver;
        _tenantDbContext = tenantDbContext;
    }

    public async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var tenantId = _tenantResolver.TenantId;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException("Tenant ID bulunamadı. Lütfen X-Tenant-ID header'ını kontrol edin.");
        }

        // Tenant bilgisini TenantDbContext'ten al
        var tenant = await _tenantDbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id.ToString() == tenantId && t.IsActive);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı veya aktif değil: {tenantId}");
        }

        // Tenant'ın connection string'i ile ApplicationDbContext oluştur
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(tenant.ConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
