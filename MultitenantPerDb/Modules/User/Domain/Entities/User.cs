using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Modules.User.Domain.Entities;

/// <summary>
/// User entity - Tenant-specific user (stored in tenant's own database)
/// TenantId is implicit - determined by which database the user is stored in
/// </summary>
public class User : BaseEntity<int>, IAggregateRoot
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core için parameterless constructor
    private User() : base()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        IsActive = true;
    }

    // Factory method
    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true
        };
    }

    // Business methods
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already inactive");

        IsActive = false;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Email = email;
        
    }
}
