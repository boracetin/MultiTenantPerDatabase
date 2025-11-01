using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetInStockProducts;

/// <summary>
/// Handler for GetInStockProductsQuery
/// Uses DTO projection with predicate filtering
/// </summary>
public class GetInStockProductsQueryHandler : IRequestHandler<GetInStockProductsQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInStockProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> Handle(GetInStockProductsQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // ✅ EFFICIENT - DTO projection with predicate
        var products = await repository.FindAsync<ProductDto>(
            p => p.Stock > 0, 
            cancellationToken
        );

        return products.ToList();
    }
}
