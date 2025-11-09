using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Generic factory interface for creating DbContext instances
/// Enables lazy initialization and multi-context support in UnitOfWork
/// Synchronous factory pattern - DbContext creation is CPU-bound (configuration, memory allocation)
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext to create (TenancyDbContext, UserDbContext, etc.)</typeparam>
public interface ITenantDbContextFactory<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Creates and returns a new instance of TDbContext
    /// For tenant-specific contexts: Resolves tenant and creates tenant-specific context
    /// For TenancyDbContext: Creates master database context
    /// Synchronous method - no I/O operations, only configuration and object creation
    /// </summary>
    TDbContext CreateDbContext();
}
