using MultitenantPerDb.Modules.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Products.Application.Commands;

public record CreateProductCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}

public record UpdateProductCommand
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public record DeleteProductCommand
{
    public int Id { get; init; }
}

public record UpdateProductStockCommand
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

