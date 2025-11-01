using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Commands;

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

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(int Id) : IRequest<bool>;

/// <summary>
/// Command to update product stock
/// </summary>
public record UpdateProductStockCommand : IRequest<ProductDto>
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

