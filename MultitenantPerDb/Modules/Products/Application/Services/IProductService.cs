using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Entities;
using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Shared.Kernel.Infrastructure;

namespace MultitenantPerDb.Modules.Products.Application.Services;

/// <summary>
/// Product service interface
/// Handles business logic for product operations
/// </summary>
public interface IProductService
{
    // Query methods
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductDtoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetProductsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetInStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default);

    // Command methods
    Task<Product> CreateProductAsync(string name, string description, decimal price, int stock, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(int productId, string? name = null, string? description = null, decimal? price = null, int? stock = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default);

    // Validation methods
    Task<bool> IsProductNameAvailableAsync(string name, CancellationToken cancellationToken = default);
}
