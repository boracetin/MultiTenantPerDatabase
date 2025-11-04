using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Identity.Application.Services;

/// <summary>
/// User service implementation
/// Uses IUnitOfWork to access Repository<User> for data access
/// UnitOfWork manages the ApplicationDbContext and ensures single instance per request
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

    public UserService(IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Gets Repository<User> from UnitOfWork
    /// UnitOfWork ensures same context instance is used for all repositories
    /// </summary>
    private IRepository<User> GetRepository()
    {
        return _unitOfWork.GetGenericRepository<User>();
    }

    #region Query Methods

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FirstOrDefaultAsync(
            u => u.Username == username && u.IsActive,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FirstOrDefaultAsync(
            u => u.Email == email && u.IsActive,
            asNoTracking: true,
            cancellationToken);
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        // ✅ Mapster projection - only DTO fields are queried
        return await repository.GetByIdAsync<UserDto>(id, cancellationToken);
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(
        int pageNumber, 
        int pageSize, 
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
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
        var repository = GetRepository();

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
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return user;
    }

    public async Task<bool> UpdateUserAsync(
        int userId, 
        string? email = null, 
        string? password = null,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
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
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> ActivateUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Aggregate Root business method
        user.Activate();

        repository.Update(user);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Aggregate Root business method
        user.Deactivate();

        repository.Update(user);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // ✅ Business rule: Can't delete active users (must deactivate first)
        if (user.IsActive)
            throw new InvalidOperationException("Cannot delete an active user. Deactivate the user first.");

        // ✅ Soft delete if supported, otherwise hard delete
        repository.SoftDelete(user);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    #endregion

    #region Validation Methods

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var exists = await repository.AnyAsync(
            u => u.Username == username, 
            cancellationToken);
        
        return !exists;
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var exists = await repository.AnyAsync(
            u => u.Email == email, 
            cancellationToken);
        
        return !exists;
    }

    #endregion
}
