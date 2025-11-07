using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProducts;

/// <summary>
/// Query to get all products
/// </summary>
public record GetProductsQuery : IRequest<List<ProductDto>>;
