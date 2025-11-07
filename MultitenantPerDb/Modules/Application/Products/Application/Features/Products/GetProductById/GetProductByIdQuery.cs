using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProductById;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
