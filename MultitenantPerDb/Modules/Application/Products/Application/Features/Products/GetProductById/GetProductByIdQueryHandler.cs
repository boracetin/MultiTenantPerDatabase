using MediatR;
using MultitenantPerDb.Modules.Application.Products.Application.DTOs;
using MultitenantPerDb.Modules.Application.Products.Application.Services;

namespace MultitenantPerDb.Modules.Application.Products.Application.Features.Products.GetProductById;

/// <summary>
/// Handler for GetProductByIdQuery
/// Uses IProductService with DTO projection for efficient database queries
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ EFFICIENT - Uses DTO projection via ProductService (only required fields selected from DB)
        var productDto = await _productService.GetProductDtoByIdAsync(request.Id, cancellationToken);

        return productDto;
    }
}
