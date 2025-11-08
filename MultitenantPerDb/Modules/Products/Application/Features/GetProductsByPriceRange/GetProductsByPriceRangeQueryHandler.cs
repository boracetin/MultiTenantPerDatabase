using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;

namespace MultitenantPerDb.Modules.Products.Application.Features.GetProductsByPriceRange;

/// <summary>
/// Handler for GetProductsByPriceRangeQuery
/// Uses IProductService for business logic
/// </summary>
public class GetProductsByPriceRangeQueryHandler : IRequestHandler<GetProductsByPriceRangeQuery, List<ProductDto>>
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public GetProductsByPriceRangeQueryHandler(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByPriceRangeQuery request, CancellationToken cancellationToken)
    {
        // ✅ ProductService handles the query
        var products = await _productService.GetProductsByPriceRangeAsync(
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            cancellationToken: cancellationToken
        );

        // Map to DTOs
        return _mapper.Map<List<ProductDto>>(products);
    }
}
