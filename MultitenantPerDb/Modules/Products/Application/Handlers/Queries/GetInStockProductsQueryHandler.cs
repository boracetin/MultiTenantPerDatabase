using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.Queries;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Handlers.Queries;

/// <summary>
/// Handler for GetInStockProductsQuery
/// </summary>
public class GetInStockProductsQueryHandler : IRequestHandler<GetInStockProductsQuery, List<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInStockProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetInStockProductsQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        var products = await repository.GetInStockProductsAsync();

        return _mapper.Map<List<ProductDto>>(products);
    }
}
