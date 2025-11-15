using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MultitenantPerDb.Core.Domain;
using Mapster;

namespace MultitenantPerDb.Core.Infrastructure;

/// <summary>
/// Generic repository implementation with advanced querying and projection capabilities
/// Supports efficient DTO projection using Mapster for optimized database queries
/// Can work with any DbContext (TenantDbContext or ApplicationDbContext)
/// </summary>
public class Repository<TEntity, TId> : IRepository<TEntity, TId> 
    where TEntity : class, IEntity<TId> 
    where TId : IEquatable<TId>
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    #region Query Methods - Entity

    /// <summary>
    /// Get entity by ID (primary key lookup)
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    /// <summary>
    /// Get all entities with optional tracking
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Get single entity matching predicate or throw exception
    /// </summary>
    public virtual async Task<TEntity> SingleAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Check if any entity matches predicate
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        return predicate == null 
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    #endregion

    #region Query Methods - DTO Projection (Efficient)

    /// <summary>
    /// Get entity by ID and project to DTO using Mapster
    /// Only selected fields are queried from database (SELECT optimization)
    /// </summary>
    public virtual async Task<TDto?> GetByIdAsync<TDto>(TId id, CancellationToken cancellationToken = default) 
        where TDto : class
    {
        return await _dbSet
            .Where(e => e.Id.Equals(id))
            .ProjectToType<TDto>() // Mapster projection - only DTO fields in SELECT
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get all entities projected to DTO using Mapster
    /// Only DTO fields are queried from database
    /// </summary>
    public virtual async Task<IEnumerable<TDto>> GetAllAsync<TDto>(CancellationToken cancellationToken = default) 
        where TDto : class
    {
        return await _dbSet
            .ProjectToType<TDto>() // Mapster projection
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find entities matching predicate and project to DTO
    /// Efficient database query - only DTO fields selected
    /// </summary>
    public virtual async Task<IEnumerable<TDto>> FindAsync<TDto>(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default) 
        where TDto : class
    {
        return await _dbSet
            .Where(predicate)
            .ProjectToType<TDto>() // Mapster projection
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get first entity matching predicate and project to DTO
    /// </summary>
    public virtual async Task<TDto?> FirstOrDefaultAsync<TDto>(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default) 
        where TDto : class
    {
        return await _dbSet
            .Where(predicate)
            .ProjectToType<TDto>() // Mapster projection
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get paged results with DTO projection
    /// Supports efficient pagination with database-level SELECT
    /// </summary>
    public virtual async Task<PagedResult<TDto>> GetPagedAsync<TDto>(
        int pageNumber, 
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default) 
        where TDto : class
    {
        var query = _dbSet.AsQueryable();

        // Apply filter
        if (predicate != null)
            query = query.Where(predicate);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering
        if (orderBy != null)
        {
            query = ascending 
                ? query.OrderBy(orderBy) 
                : query.OrderByDescending(orderBy);
        }

        // Apply pagination and projection
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<TDto>() // Mapster projection
            .ToListAsync(cancellationToken);

        return new PagedResult<TDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    #endregion

    #region Command Methods

    /// <summary>
    /// Add new entity to repository
    /// </summary>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.SetCreatedAt();
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Add multiple entities to repository
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.SetCreatedAt();
        }
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Update existing entity
    /// </summary>
    public virtual void Update(TEntity entity)
    {
        entity.SetUpdatedAt();
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Update multiple entities
    /// </summary>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.SetUpdatedAt();
        }
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Soft delete - marks entity as deleted
    /// </summary>
    public virtual void Delete(TEntity entity)
    {
        entity.SetDeleted();
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Soft delete multiple entities - marks entities as deleted
    /// </summary>
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.SetDeleted();
        }
        _dbSet.UpdateRange(entities);
    }

    #endregion

    #region Advanced Query Methods

    /// <summary>
    /// Get queryable for complex queries
    /// Use with caution - prefer specific repository methods
    /// </summary>
    public virtual IQueryable<TEntity> GetQueryable(bool asNoTracking = true)
    {
        return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
    }

    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FromSqlRawAsync(
        string sql, 
        params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }

    /// <summary>
    /// Get entities with includes (eager loading)
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetWithIncludesAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        // Apply includes
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // Apply filter
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    #endregion
}
