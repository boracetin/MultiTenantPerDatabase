using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Data;
using MultitenantPerDb.Models;

namespace MultitenantPerDb.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _dbSet
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetInStockProductsAsync()
    {
        return await _dbSet
            .Where(p => p.Stock > 0)
            .ToListAsync();
    }
}
