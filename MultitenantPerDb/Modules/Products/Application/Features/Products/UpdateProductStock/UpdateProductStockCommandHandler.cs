using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.UpdateProductStock;

/// <summary>
/// Handler for UpdateProductStockCommand
/// </summary>
public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductStockCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // Get existing product
        var product = await repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        // Update stock using domain method
        product.UpdateStock(request.Quantity);

        // Save changes
        repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<ProductDto>(product);
    }
}
