using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand : IRequest<ProductDto>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
