using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetInStockProducts;

/// <summary>
/// Handler for GetInStockProductsQuery
/// Uses IProductService for business logic
/// </summary>
public class GetInStockProductsQueryHandler : IRequestHandler<GetInStockProductsQuery, List<ProductDto>>
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public GetInStockProductsQueryHandler(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetInStockProductsQuery request, CancellationToken cancellationToken)
    {
        // ✅ ProductService handles the query
        var products = await _productService.GetInStockProductsAsync(cancellationToken);

        // Map to DTOs
        return _mapper.Map<List<ProductDto>>(products);
    }
}
