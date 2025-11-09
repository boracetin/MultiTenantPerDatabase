namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Marker interface for commands that require distributed transactions across multiple databases
/// Use this when a command needs to modify both:
/// - TenancyDbContext (master database)
/// - Tenant-specific context (ProductsDbContext or IdentityDbContext)
/// 
/// Example: Creating a new tenant AND initializing tenant database
/// </summary>
public interface IDistributedTransactionalCommand
{
}
