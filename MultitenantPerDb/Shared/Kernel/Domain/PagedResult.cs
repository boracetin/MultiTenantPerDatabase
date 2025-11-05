namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Paged result model for pagination
/// </summary>
public class PagedResult<TEntity>
{
    public IEnumerable<TEntity> Items { get; set; } = new List<TEntity>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
