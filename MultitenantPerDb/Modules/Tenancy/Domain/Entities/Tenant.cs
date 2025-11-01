using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Identity.Domain.Entities;

namespace MultitenantPerDb.Modules.Tenancy.Domain.Entities;

/// <summary>
/// Tenant aggregate root - Multi-tenancy için ana entity
/// </summary>
public class Tenant : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string ConnectionString { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    // EF Core için parameterless constructor
    private Tenant() : base()
    {
        Name = string.Empty;
        ConnectionString = string.Empty;
        IsActive = true;
    }

    // Factory method
    public static Tenant Create(string name, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        return new Tenant
        {
            Name = name,
            ConnectionString = connectionString,
            IsActive = true
        };
    }

    // Business methods
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Tenant is already active");

        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Tenant is already inactive");

        IsActive = false;
        SetUpdatedAt();
    }

    public void UpdateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        ConnectionString = connectionString;
        SetUpdatedAt();
    }
}
