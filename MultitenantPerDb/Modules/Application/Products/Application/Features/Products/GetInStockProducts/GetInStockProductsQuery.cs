using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetInStockProducts;

/// <summary>
/// Query to get products that are in stock
/// </summary>
public record GetInStockProductsQuery : IRequest<List<ProductDto>>;
