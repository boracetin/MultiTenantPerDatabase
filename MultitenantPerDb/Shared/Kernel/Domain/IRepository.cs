using System.Linq.Expressions;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Generic repository interface with advanced querying and DTO projection capabilities
/// Follows Repository Pattern with CQRS-friendly methods
/// TEntity: Entity type (Product, User, Tenant, etc.)
/// Works with any DbContext (ApplicationDbContext, MainDbContext, etc.)
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    #region Query Methods - Entity
    
    /// <summary>
    /// Get entity by ID (primary key lookup)
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all entities with optional tracking
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get single entity matching predicate or throw exception
    /// </summary>
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if any entity matches predicate
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    #endregion

    #region Query Methods - DTO Projection (Efficient)
    
    /// <summary>
    /// Get entity by ID and project to DTO using Mapster
    /// Only selected fields are queried from database (SELECT optimization)
    /// </summary>
    Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Get all entities projected to DTO using Mapster
    /// Only DTO fields are queried from database
    /// </summary>
    Task<IEnumerable<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Find entities matching predicate and project to DTO
    /// Efficient database query - only DTO fields selected
    /// </summary>
    Task<IEnumerable<TDto>> FindAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Get first entity matching predicate and project to DTO
    /// </summary>
    Task<TDto?> FirstOrDefaultAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Get paged results with DTO projection
    /// Supports efficient pagination with database-level SELECT
    /// </summary>
    Task<PagedResult<TDto>> GetPagedAsync<TDto>(
        int pageNumber, 
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default) where TDto : class;
    
    #endregion

    #region Command Methods
    
    /// <summary>
    /// Add new entity to repository
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add multiple entities to repository
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update existing entity
    /// </summary>
    void Update(TEntity entity);
    
    /// <summary>
    /// Update multiple entities
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Remove entity from repository
    /// </summary>
    void Remove(TEntity entity);
    
    /// <summary>
    /// Remove multiple entities from repository
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Soft delete - marks entity as deleted (if supported)
    /// </summary>
    void SoftDelete(TEntity entity);
    
    #endregion

    #region Advanced Query Methods
    
    /// <summary>
    /// Get queryable for complex queries
    /// Use with caution - prefer specific repository methods
    /// </summary>
    IQueryable<TEntity> GetQueryable(bool asNoTracking = true);
    
    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, params object[] parameters);
    
    /// <summary>
    /// Get entities with includes (eager loading)
    /// </summary>
    Task<IEnumerable<TEntity>> GetWithIncludesAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);
    
    #endregion
}
