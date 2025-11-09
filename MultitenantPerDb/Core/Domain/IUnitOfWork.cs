using Microsoft.EntityFrameworkCore;

namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Base interface for Unit of Work - non-generic transaction management
/// Used by DistributedTransactionBehavior to manage multiple UnitOfWork instances
/// </summary>
public interface IUnitOfWorkBase
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work interface for managing transactions and repositories
/// Generic interface - works with specific DbContext type (TDbContext)
/// Transaction yönetimi TransactionBehavior tarafından yapılır
/// </summary>
public interface IUnitOfWork<TDbContext> : IUnitOfWorkBase, IDisposable
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets a Repository<TEntity, TId> for entity TEntity with primary key type TId
    /// TEntity: Entity type (Product, User, Tenant, etc.)
    /// TId: Primary key type (int, Guid, string, etc.)
    /// Works with any DbContext injected via factory
    /// </summary>
    IRepository<TEntity, TId> GetRepository<TEntity, TId>() 
        where TEntity : class, IEntity<TId> 
        where TId : IEquatable<TId>;
}
