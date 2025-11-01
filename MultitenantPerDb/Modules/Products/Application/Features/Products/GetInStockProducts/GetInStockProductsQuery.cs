using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetInStockProducts;

/// <summary>
/// Query to get products that are in stock
/// </summary>
public record GetInStockProductsQuery : IRequest<List<ProductDto>>;
