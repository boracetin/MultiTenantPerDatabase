using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Identity.Domain.Repositories;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;

namespace MultitenantPerDb.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// User repository implementation using Master DB (TenantDbContext)
/// Tenant-independent repository for authentication
/// Uses TenantDbContext to access Users table WITHOUT requiring TenantId
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly TenantDbContext _context;

    public UserRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
