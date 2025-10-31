using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Infrastructure.Persistence;
using MultitenantPerDb.Domain.Entities;
using MultitenantPerDb.Domain.Repositories;

namespace MultitenantPerDb.Infrastructure.Persistence;

/// <summary>
/// Product repository implementation with domain-specific queries
/// </summary>
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

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _dbSet
            .Where(p => p.Stock > 0 && p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }
}
