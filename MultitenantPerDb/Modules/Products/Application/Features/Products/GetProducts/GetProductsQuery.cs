using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetProducts;

/// <summary>
/// Query to get all products
/// </summary>
public record GetProductsQuery : IRequest<List<ProductDto>>;
