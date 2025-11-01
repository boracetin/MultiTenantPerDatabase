using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using Mapster;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Generic repository implementation with advanced querying and projection capabilities
/// Supports efficient DTO projection using Mapster for optimized database queries
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Query Methods - Entity

    /// <summary>
    /// Get entity by ID (primary key lookup)
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Get all entities with optional tracking
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find entities matching predicate
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, 
        bool asNoTracking = true, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Get single entity matching predicate or throw exception
    /// </summary>
    public virtual async Task<T> SingleAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Check if any entity matches predicate
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Count entities matching predicate
    /// </summary>
    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null, 
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
    public virtual async Task<TDto?> GetByIdAsync<TDto>(int id, CancellationToken cancellationToken = default) 
        where TDto : class
    {
        return await _dbSet
            .Where(e => EF.Property<int>(e, "Id") == id)
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
        Expression<Func<T, bool>> predicate, 
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
        Expression<Func<T, bool>> predicate, 
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
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
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
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Add multiple entities to repository
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Update existing entity
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Update multiple entities
    /// </summary>
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Remove entity from repository
    /// </summary>
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Remove multiple entities from repository
    /// </summary>
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Soft delete - marks entity as deleted (if supported)
    /// </summary>
    public virtual void SoftDelete(T entity)
    {
        // Check if entity has IsDeleted property
        var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            isDeletedProperty.SetValue(entity, true);
            _dbSet.Update(entity);
        }
        else
        {
            // Fallback to hard delete
            Remove(entity);
        }
    }

    #endregion

    #region Advanced Query Methods

    /// <summary>
    /// Get queryable for complex queries
    /// Use with caution - prefer specific repository methods
    /// </summary>
    public virtual IQueryable<T> GetQueryable(bool asNoTracking = true)
    {
        return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
    }

    /// <summary>
    /// Execute raw SQL query
    /// </summary>
    public virtual async Task<IEnumerable<T>> FromSqlRawAsync(
        string sql, 
        params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }

    /// <summary>
    /// Get entities with includes (eager loading)
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetWithIncludesAsync(
        Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

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

/// <summary>
/// Paged result model for pagination
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
