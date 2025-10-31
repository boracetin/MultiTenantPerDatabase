using MultitenantPerDb.Domain.Common;
using MultitenantPerDb.Domain.Events;

namespace MultitenantPerDb.Domain.Entities;

/// <summary>
/// User entity - Tenant'a ait kullanıcı
/// </summary>
public class User : BaseEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public int TenantId { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public Tenant? Tenant { get; private set; }

    // EF Core için parameterless constructor
    private User() : base()
    {
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        IsActive = true;
    }

    // Factory method
    public static User Create(string username, string email, string passwordHash, int tenantId)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        if (tenantId <= 0)
            throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            TenantId = tenantId,
            IsActive = true
        };
    }

    // Business methods
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active");

        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already inactive");

        IsActive = false;
        SetUpdatedAt();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Email = email;
        SetUpdatedAt();
    }

    public void RaiseLoginEvent()
    {
        AddDomainEvent(new UserLoggedInEvent(this));
    }
}
