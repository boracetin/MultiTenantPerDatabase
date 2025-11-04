using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Generic factory interface for creating DbContext instances
/// Enables lazy initialization and multi-context support in UnitOfWork
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext to create (MainDbContext, ApplicationDbContext, etc.)</typeparam>
public interface ITenantDbContextFactory<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Creates and returns a new instance of TDbContext
    /// For ApplicationDbContext: Resolves tenant and creates tenant-specific context
    /// For MainDbContext: Creates master database context
    /// </summary>
    Task<TDbContext> CreateDbContextAsync();
}
