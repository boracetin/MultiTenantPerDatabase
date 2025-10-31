using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Infrastructure.Persistence;
using MultitenantPerDb.Domain.Entities;
using MultitenantPerDb.Domain.Repositories;

namespace MultitenantPerDb.Infrastructure.Persistence;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TenantDbContext context) : base((ApplicationDbContext)(object)context)
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

    public async Task<IEnumerable<User>> GetUsersByTenantIdAsync(int tenantId)
    {
        return await _dbSet.Where(u => u.TenantId == tenantId).ToListAsync();
    }
}
