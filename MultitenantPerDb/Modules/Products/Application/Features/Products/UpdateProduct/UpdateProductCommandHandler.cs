using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.UpdateProduct;

/// <summary>
/// Handler for UpdateProductCommand
/// Uses IProductService for business logic
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // ProductService handles business logic and validation
        await _productService.UpdateProductAsync(
            productId: request.Id,
            name: request.Name,
            description: request.Description,
            price: request.Price,
            stock: null, // Stock not updated here
            cancellationToken: cancellationToken
        );

        // Get updated product DTO
        var productDto = await _productService.GetProductDtoByIdAsync(request.Id, cancellationToken);
        
        if (productDto == null)
            throw new InvalidOperationException($"Product with ID {request.Id} not found after update");

        return productDto;
    }
}
