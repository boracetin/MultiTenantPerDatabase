using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.GetProducts;

/// <summary>
/// Handler for GetProductsQuery
/// Uses DTO projection for efficient database queries
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // ✅ EFFICIENT - Uses DTO projection
        var products = await repository.GetAllAsync<ProductDto>(cancellationToken);

        return products.ToList();
    }
}
