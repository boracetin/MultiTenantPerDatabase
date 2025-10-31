using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Domain.Repositories;

/// <summary>
/// Tenant repository interface
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByNameAsync(string name);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
}
