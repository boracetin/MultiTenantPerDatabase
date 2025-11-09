using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Modules.Products.Infrastructure.Persistence;
using MultitenantPerDb.Core.Application;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;

namespace MultitenantPerDb.Modules.Products.Application.Services;

/// <summary>
/// Product service implementation
/// Uses IUnitOfWork to access Repository<Product> for data access
/// UnitOfWork manages the ProductsDbContext and ensures single instance per request
/// Inherits from BaseService to enforce ICanAccessUnitOfWork constraint (checked by MTDB003 analyzer)
/// </summary>
public class ProductService : BaseService, IProductService
{
    private readonly IUnitOfWork<ProductsDbContext> _unitOfWork;

    public ProductService(IUnitOfWork<ProductsDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets Repository<Product, int> from UnitOfWork
    /// UnitOfWork ensures same context instance is used for all repositories
    /// </summary>
    private IRepository<Product, int> GetRepository()
    {
        return _unitOfWork.GetRepository<Product, int>();
    }

    #region Query Methods

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ProductDto?> GetProductDtoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        // ✅ Tip vermeden kullanım - overload sayesinde
        return await repository.GetByIdAsync<ProductDto>(id, cancellationToken);
    }

    public async Task<PagedResult<ProductDto>> GetProductsPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        // ✅ Built-in pagination with DTO projection
        return await repository.GetPagedAsync<ProductDto>(
            pageNumber: pageNumber,
            pageSize: pageSize,
            predicate: null,
            orderBy: p => p.Name,
            ascending: true,
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice, 
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var query = repository.GetQueryable()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetInStockProductsAsync(CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        return await repository.FindAsync(
            p => p.Stock > 0,
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(
        int threshold = 10, 
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var query = repository.GetQueryable()
            .Where(p => p.Stock > 0 && p.Stock <= threshold)
            .OrderBy(p => p.Stock);
        return await query.ToListAsync(cancellationToken);
    }

    #endregion

    #region Command Methods

    public async Task<Product> CreateProductAsync(
        string name, 
        string description, 
        decimal price, 
        int stock,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();

        // ✅ Business validation
        var nameExists = await repository.AnyAsync(
            p => p.Name == name, 
            cancellationToken);
        
        if (nameExists)
            throw new InvalidOperationException($"Product name '{name}' already exists");

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        // ✅ Aggregate Root factory method
        var product = Product.Create(name, description, price, stock);

        // ✅ Save via repository
        await repository.AddAsync(product, cancellationToken);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return product;
    }

    public async Task<bool> UpdateProductAsync(
        int productId, 
        string? name = null, 
        string? description = null, 
        decimal? price = null,
        int? stock = null,
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var product = await repository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            throw new InvalidOperationException($"Product with ID {productId} not found");

        // ✅ Business validation for name uniqueness
        var finalName = name ?? product.Name;
        var finalDescription = description ?? product.Description;
        var finalPrice = price ?? product.Price;

        if (!string.IsNullOrEmpty(name) && name != product.Name)
        {
            var nameExists = await repository.AnyAsync(
                p => p.Name == name && p.Id != productId, 
                cancellationToken);
            
            if (nameExists)
                throw new InvalidOperationException($"Product name '{name}' is already in use");
        }

        if (finalPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        // ✅ Aggregate Root business method - UpdateDetails handles name, description, price
        product.UpdateDetails(finalName, finalDescription, finalPrice);

        // Handle stock separately if provided
        if (stock.HasValue)
        {
            if (stock.Value < 0)
                throw new ArgumentException("Stock cannot be negative", nameof(stock));
            
            var stockDifference = stock.Value - product.Stock;
            product.UpdateStock(stockDifference);
        }

        repository.Update(product);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> UpdateStockAsync(
        int productId, 
        int quantity, 
        CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var product = await repository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            throw new InvalidOperationException($"Product with ID {productId} not found");

        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        // ✅ Aggregate Root business method
        product.UpdateStock(quantity);

        repository.Update(product);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var product = await repository.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            throw new InvalidOperationException($"Product with ID {productId} not found");

        // ✅ Soft delete - marks entity as deleted
        repository.Delete(product);
        // Note: SaveChangesAsync is called by Handler via IUnitOfWork

        return true;
    }

    #endregion

    #region Validation Methods

    public async Task<bool> IsProductNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        var repository = GetRepository();
        var exists = await repository.AnyAsync(
            p => p.Name == name, 
            cancellationToken);
        
        return !exists;
    }

    #endregion
}
