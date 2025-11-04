using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

/// <summary>
/// Factory interface for creating tenant-specific DbContext
/// </summary>
public interface ITenantDbContextFactory
{
    Task<ApplicationDbContext> CreateDbContextAsync();
}

/// <summary>
/// Creates ApplicationDbContext with tenant-specific connection string
/// </summary>
public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly ITenantResolver _tenantResolver;
    private readonly MainDbContext _mainDbContext;

    public TenantDbContextFactory(ITenantResolver tenantResolver, MainDbContext mainDbContext)
    {
        _tenantResolver = tenantResolver;
        _mainDbContext = mainDbContext;
    }

    public async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var tenantIdentifier = _tenantResolver.TenantId;
        
        if (string.IsNullOrEmpty(tenantIdentifier))
        {
            throw new InvalidOperationException("Tenant bilgisi bulunamadı. Subdomain, JWT claim, Header (X-Tenant-ID) veya Query string kullanın.");
        }

        // Tenant'ı ID veya Name ile bul
        // Subdomain'den gelen "tenant1" gibi bir string olabilir (Name)
        // veya JWT claim'den gelen "1" gibi bir ID olabilir
        var tenant = await _mainDbContext.Tenants
            .FirstOrDefaultAsync(t => 
                (t.Id.ToString() == tenantIdentifier || t.Name.ToLower() == tenantIdentifier.ToLower()) 
                && t.IsActive);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant bulunamadı veya aktif değil: {tenantIdentifier}");
        }

        // Tenant'ın connection string'i ile ApplicationDbContext oluştur
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(tenant.ConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
