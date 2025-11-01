using MediatR;
using MapsterMapper;
using MultitenantPerDb.Modules.Products.Application.Commands;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Handlers.Commands;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // Get existing product
        var product = await repository.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.Id} not found");
        }

        // Update using domain methods (encapsulation)
        product.UpdateDetails(request.Name, request.Description, request.Price);

        // Save changes
        repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<ProductDto>(product);
    }
}
