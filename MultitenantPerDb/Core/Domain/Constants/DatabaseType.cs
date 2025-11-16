namespace MultitenantPerDb.Core.Domain.Constants;

/// <summary>
/// Database type for transaction grouping
/// Used to identify which physical database a DbContext belongs to
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// No database - read-only or no persistence
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Main database - Master/System data
    /// Example: TenancyDbContext, IdentityDbContext (master)
    /// </summary>
    Main = 1,
    
    /// <summary>
    /// Application database - Tenant-specific data
    /// Example: ProductsDbContext, UserDbContext (per-tenant)
    /// </summary>
    Application = 2
}
