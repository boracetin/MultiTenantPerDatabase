using System.Linq.Expressions;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Generic repository interface with advanced querying and DTO projection capabilities
/// Follows Repository Pattern with CQRS-friendly methods
/// </summary>
public interface IRepository<T> where T : class
{
    #region Query Methods - Entity
    
    /// <summary>
    /// Get entity by ID (primary key lookup)
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all entities with optional tracking
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get single entity matching predicate or throw exception
    /// </summary>
    Task<T> SingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if any entity matches predicate
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
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
    Task<IEnumerable<TDto>> FindAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Get first entity matching predicate and project to DTO
    /// </summary>
    Task<TDto?> FirstOrDefaultAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class;
    
    /// <summary>
    /// Get paged results with DTO projection
    /// Supports efficient pagination with database-level SELECT
    /// </summary>
    Task<PagedResult<TDto>> GetPagedAsync<TDto>(
        int pageNumber, 
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default) where TDto : class;
    
    #endregion

    #region Command Methods
    
    /// <summary>
    /// Add new entity to repository
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add multiple entities to repository
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update existing entity
    /// </summary>
    void Update(T entity);
    
    /// <summary>
    /// Update multiple entities
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);
    
    /// <summary>
    /// Remove entity from repository
    /// </summary>
    void Remove(T entity);
    
    /// <summary>
    /// Remove multiple entities from repository
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);
    
    /// <summary>
    /// Soft delete - marks entity as deleted (if supported)
    /// </summary>
    void SoftDelete(T entity);
    
    #endregion

    #region Advanced Query Methods
    
    /// <summary>
    /// Get queryable for complex queries
    /// Use with caution - prefer specific repository methods
    /// </summary>
    IQueryable<T> GetQueryable(bool asNoTracking = true);
    
    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters);
    
    /// <summary>
    /// Get entities with includes (eager loading)
    /// </summary>
    Task<IEnumerable<T>> GetWithIncludesAsync(
        Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includes);
    
    #endregion
}
