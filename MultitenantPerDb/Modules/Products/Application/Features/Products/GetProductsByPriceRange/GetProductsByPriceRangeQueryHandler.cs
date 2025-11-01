using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetProductsByPriceRange;

/// <summary>
/// Handler for GetProductsByPriceRangeQuery
/// Uses DTO projection with price range filtering
/// </summary>
public class GetProductsByPriceRangeQueryHandler : IRequestHandler<GetProductsByPriceRangeQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsByPriceRangeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByPriceRangeQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // ✅ EFFICIENT - DTO projection with price range filter
        var products = await repository.FindAsync<ProductDto>(
            p => p.Price >= request.MinPrice && p.Price <= request.MaxPrice,
            cancellationToken
        );

        return products.ToList();
    }
}
