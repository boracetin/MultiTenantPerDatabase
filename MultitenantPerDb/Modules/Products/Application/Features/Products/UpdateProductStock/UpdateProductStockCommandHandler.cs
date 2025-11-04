using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.UpdateProductStock;

/// <summary>
/// Handler for UpdateProductStockCommand
/// Uses IProductService for business logic
/// </summary>
public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductDto>
{
    private readonly IProductService _productService;

    public UpdateProductStockCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        // ProductService handles business logic and validation
        await _productService.UpdateStockAsync(
            productId: request.ProductId,
            quantity: request.Quantity,
            cancellationToken: cancellationToken
        );

        // Get updated product DTO
        var productDto = await _productService.GetProductDtoByIdAsync(request.ProductId, cancellationToken);
        
        if (productDto == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found after update");

        return productDto;
    }
}
