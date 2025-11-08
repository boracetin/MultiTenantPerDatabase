using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.GetProductById;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
