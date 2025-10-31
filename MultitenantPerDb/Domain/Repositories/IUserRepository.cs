using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Domain.Repositories;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByTenantIdAsync(int tenantId);
}
