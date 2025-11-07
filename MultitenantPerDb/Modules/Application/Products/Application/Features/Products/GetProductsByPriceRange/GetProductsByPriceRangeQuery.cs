using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProductsByPriceRange;

/// <summary>
/// Query to get products within a price range
/// </summary>
public record GetProductsByPriceRangeQuery(decimal MinPrice, decimal MaxPrice) : IRequest<List<ProductDto>>;
