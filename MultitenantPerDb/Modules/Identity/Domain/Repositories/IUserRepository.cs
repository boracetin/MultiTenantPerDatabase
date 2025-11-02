using MultitenantPerDb.Modules.Identity.Domain.Entities;

namespace MultitenantPerDb.Modules.Identity.Domain.Repositories;

/// <summary>
/// User repository interface for Master DB (tenant-independent)
/// Simplified interface without generic repository inheritance
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
