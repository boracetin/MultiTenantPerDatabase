using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// User repository implementation using Master DB (TenantDbContext)
/// Tenant-independent repository for authentication
/// Uses TenantDbContext to access Users table WITHOUT requiring TenantId
/// Inherits from Repository<User> for common CRUD operations
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TenantDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _dbSet.AddAsync(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
