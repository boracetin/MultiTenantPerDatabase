using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.UpdateProductStock;

/// <summary>
/// Command to update product stock
/// </summary>
public record UpdateProductStockCommand : IRequest<ProductDto>
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}
