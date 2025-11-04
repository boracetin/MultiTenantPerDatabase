using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

/// <summary>
/// Tenant repository implementation
/// Uses MainDbContext (Master DB)
/// </summary>
public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(MainDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Subdomain == subdomain);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
    {
        return await _dbSet.Where(t => t.IsActive).ToListAsync();
    }
}
