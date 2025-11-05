using MultitenantPerDb.Modules.Identity.Application.DTOs;
using MultitenantPerDb.Modules.Identity.Domain.Entities;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Identity.Application.Services;

/// <summary>
/// User business logic service
/// Handles user-related operations with business rules and validation
/// </summary>
public interface IUserService
{
    // Query methods
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserDtoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<UserDto>> GetUsersPagedAsync(int pageNumber, int pageSize, bool? isActive = null, CancellationToken cancellationToken = default);
    
    // Command methods
    Task<User> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(int userId, string? email = null, string? password = null, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default);
    
    // Validation methods
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}
