using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Shared.Kernel.Application.Abstractions;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.CreateProduct;

/// <summary>
/// Command to create a new product
/// Uses ApplicationDbContext for tenant-specific data
/// Implements IApplicationDbTransactionalCommand to enable database transaction with ApplicationDbContext
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>, IApplicationDbTransactionalCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
