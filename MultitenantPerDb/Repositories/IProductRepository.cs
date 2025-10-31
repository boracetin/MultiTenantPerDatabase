using MultitenantPerDb.Models;

namespace MultitenantPerDb.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> GetInStockProductsAsync();
}
