using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Identity.Domain.Entities;

namespace MultitenantPerDb.Modules.Identity.Domain.Repositories;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByTenantIdAsync(int tenantId);
}
