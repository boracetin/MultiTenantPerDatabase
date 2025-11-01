using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Queries;

/// <summary>
/// Query to get all products
/// </summary>
public record GetAllProductsQuery : IRequest<List<ProductDto>>
{
}

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

/// <summary>
/// Query to get products that are in stock
/// </summary>
public record GetInStockProductsQuery : IRequest<List<ProductDto>>
{
}

/// <summary>
/// Query to get products within a price range
/// </summary>
public record GetProductsByPriceRangeQuery(decimal MinPrice, decimal MaxPrice) : IRequest<List<ProductDto>>;

