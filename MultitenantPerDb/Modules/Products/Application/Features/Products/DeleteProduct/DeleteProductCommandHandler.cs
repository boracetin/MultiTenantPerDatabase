using MediatR;
using MultitenantPerDb.Modules.Products.Domain.Repositories;
using MultitenantPerDb.Shared.Kernel.Domain;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<IProductRepository>();
        
        // Get existing product
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return false;
        }

        // Delete product
        repository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
