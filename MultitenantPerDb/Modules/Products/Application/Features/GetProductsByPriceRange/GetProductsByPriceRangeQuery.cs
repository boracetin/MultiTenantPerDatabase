using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.GetProductsByPriceRange;

/// <summary>
/// Query to get products within a price range
/// </summary>
public record GetProductsByPriceRangeQuery(decimal MinPrice, decimal MaxPrice) : IRequest<List<ProductDto>>;
