using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Identity.Application.Services;

/// <summary>
/// User service implementation
/// Uses Repository<User> for data access and implements business logic
/// Creates tenant-specific ApplicationDbContext using TenantDbContextFactory
/// </summary>
public class UserService : IUserService
{
    private readonly ITenantDbContextFactory _dbContextFactory;
    private ApplicationDbContext? _context;
    private Repository<User>? _userRepository;

    public UserService(ITenantDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Lazily creates the ApplicationDbContext and Repository<User>
    /// </summary>
    private async Task<Repository<User>> GetRepositoryAsync()
    {
        if (_userRepository == null)
        {
            _context = await _dbContextFactory.CreateDbContextAsync();
            _userRepository = new Repository<User>(_context);
        }
        return _userRepository;
    }

    #region Query Methods

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        return await repository.FirstOrDefaultAsync(
            u => u.Username == username && u.IsActive,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        return await repository.FirstOrDefaultAsync(
            u => u.Email == email && u.IsActive,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        // ✅ Mapster projection - only DTO fields are queried
        return await repository.GetByIdAsync<UserDto>(id, cancellationToken);
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(
        int pageNumber, 
        int pageSize, 
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        // ✅ Built-in pagination with DTO projection
        return await repository.GetPagedAsync<UserDto>(
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: isActive.HasValue ? u => u.IsActive == isActive.Value : null,
            orderBy: u => u.Username,
            ascending: true,
            cancellationToken: cancellationToken);
    }

    #endregion

    #region Command Methods

    public async Task<User> CreateUserAsync(
        string username, 
        string email, 
        string password,
        CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();

        // ✅ Business validation
        var usernameExists = await repository.AnyAsync(
            u => u.Username == username, 
            cancellationToken);
        
        if (usernameExists)
            throw new InvalidOperationException($"Username '{username}' already exists");

        var emailExists = await repository.AnyAsync(
            u => u.Email == email, 
            cancellationToken);
        
        if (emailExists)
            throw new InvalidOperationException($"Email '{email}' already exists");

        // ✅ Aggregate Root factory method
        var user = User.Create(username, email, password); // Password should be hashed in production

        // ✅ Save via repository
        await repository.AddAsync(user, cancellationToken);
        
        if (_context != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return user;
    }

    public async Task<bool> UpdateUserAsync(
        int userId, 
        string? email = null, 
        string? password = null,
        CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Business validation for email uniqueness
        if (!string.IsNullOrEmpty(email) && email != user.Email)
        {
            var emailExists = await repository.AnyAsync(
                u => u.Email == email && u.Id != userId, 
                cancellationToken);
            
            if (emailExists)
                throw new InvalidOperationException($"Email '{email}' is already in use");

            // ✅ Aggregate Root business method
            user.UpdateEmail(email);
        }

        // ✅ Aggregate Root business method
        if (!string.IsNullOrEmpty(password))
        {
            user.ChangePassword(password); // Should hash in production
        }

        repository.Update(user);
        
        if (_context != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> ActivateUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Aggregate Root business method
        user.Activate();

        repository.Update(user);
        
        if (_context != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Aggregate Root business method
        user.Deactivate();

        repository.Update(user);
        
        if (_context != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Business rule: Can't delete active users (must deactivate first)
        if (user.IsActive)
            throw new InvalidOperationException("Cannot delete an active user. Deactivate the user first.");

        // ✅ Soft delete if supported, otherwise hard delete
        repository.SoftDelete(user);
        
        if (_context != null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    #endregion

    #region Validation Methods

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var exists = await repository.AnyAsync(
            u => u.Username == username, 
            cancellationToken);
        
        return !exists;
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync();
        var exists = await repository.AnyAsync(
            u => u.Email == email, 
            cancellationToken);
        
        return !exists;
    }

    #endregion
}
