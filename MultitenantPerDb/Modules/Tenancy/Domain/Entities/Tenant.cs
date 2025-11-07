using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Tenancy.Domain.Entities;

/// <summary>
/// Tenant aggregate root - Multi-tenancy için ana entity
/// Includes branding and customization settings for subdomain-based UI
/// Note: Users are NOT in MainDbContext - they're in tenant-specific ApplicationDbContext
/// </summary>
public class Tenant : BaseEntity<int>, IAggregateRoot
{
    public string Name { get; private set; }
    public string ConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    
    // Subdomain for tenant identification (e.g., "tenant1" in tenant1.myapp.com)
    public string? Subdomain { get; private set; }
    
    // Branding & Customization
    public string? DisplayName { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BackgroundImageUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public string? CustomCss { get; private set; }

    // EF Core için parameterless constructor
    private Tenant() : base()
    {
        Name = string.Empty;
        ConnectionString = string.Empty;
        IsActive = true;
    }

    // Factory method
    public static Tenant Create(string name, string connectionString, string? subdomain = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        return new Tenant
        {
            Name = name,
            ConnectionString = connectionString,
            Subdomain = subdomain?.ToLowerInvariant(),
            IsActive = true,
            DisplayName = name // Default to Name
        };
    }

    // Business methods
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Tenant is already active");

        IsActive = true;
        
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Tenant is already inactive");

        IsActive = false;
        
    }

    public void UpdateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        ConnectionString = connectionString;
        
    }

    public void UpdateSubdomain(string? subdomain)
    {
        Subdomain = subdomain?.ToLowerInvariant();
        
    }

    public void UpdateBranding(
        string? displayName = null,
        string? logoUrl = null,
        string? backgroundImageUrl = null,
        string? primaryColor = null,
        string? secondaryColor = null,
        string? customCss = null)
    {
        if (displayName != null) DisplayName = displayName;
        if (logoUrl != null) LogoUrl = logoUrl;
        if (backgroundImageUrl != null) BackgroundImageUrl = backgroundImageUrl;
        if (primaryColor != null) PrimaryColor = primaryColor;
        if (secondaryColor != null) SecondaryColor = secondaryColor;
        if (customCss != null) CustomCss = customCss;
        
        
    }

    public void UpdateDetails(string name, string subdomain, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        Name = name;
        Subdomain = subdomain?.ToLowerInvariant();
        ConnectionString = connectionString;
        
        
    }
}
