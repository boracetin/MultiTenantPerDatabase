using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Domain.Repositories;

/// <summary>
/// Product repository interface with domain-specific methods
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> GetInStockProductsAsync();
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
}
