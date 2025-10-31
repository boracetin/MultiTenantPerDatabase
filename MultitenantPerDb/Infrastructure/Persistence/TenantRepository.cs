using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Infrastructure.Persistence;
using MultitenantPerDb.Domain.Entities;
using MultitenantPerDb.Domain.Repositories;

namespace MultitenantPerDb.Infrastructure.Persistence;

/// <summary>
/// Tenant repository implementation
/// </summary>
public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(TenantDbContext context) : base((ApplicationDbContext)(object)context)
    {
    }

    public async Task<Tenant?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
    {
        return await _dbSet.Where(t => t.IsActive).ToListAsync();
    }
}
