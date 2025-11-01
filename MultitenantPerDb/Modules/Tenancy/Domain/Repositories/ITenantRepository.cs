using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;

namespace MultitenantPerDb.Modules.Tenancy.Domain.Repositories;

/// <summary>
/// Tenant repository interface
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByNameAsync(string name);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
}
