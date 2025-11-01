using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
