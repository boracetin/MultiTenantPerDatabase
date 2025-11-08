using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Application.Services;
using MultitenantPerDb.Core.Infrastructure;

namespace MultitenantPerDb.Modules.Products.Application.Features.GetProducts;

/// <summary>
/// Handler for GetProductsQuery
/// Uses IProductService with DTO projection and pagination
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IProductService _productService;

    public GetProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // ✅ EFFICIENT - Uses DTO projection with pagination
        var pagedResult = await _productService.GetProductsPagedAsync(
            pageNumber: 1,
            pageSize: 1000, // Large page size for "get all" behavior
            cancellationToken: cancellationToken
        );

        return pagedResult.Items.ToList();
    }
}
