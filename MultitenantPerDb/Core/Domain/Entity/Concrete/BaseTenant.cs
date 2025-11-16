
namespace MultitenantPerDb.Core.Domain.Entity.Concrete;

/// <summary>
/// Tenant aggregate root - Multi-tenancy için ana entity
/// Includes branding and customization settings for subdomain-based UI
/// Note: Users are NOT in TenancyDbContext - they're in tenant-specific ApplicationDbContext
/// </summary>
public class BaseTenant : BaseEntity<int>
{
    public string Name { get; protected set; } = string.Empty;
    public string ConnectionString { get; protected set; } = string.Empty;
    
}
