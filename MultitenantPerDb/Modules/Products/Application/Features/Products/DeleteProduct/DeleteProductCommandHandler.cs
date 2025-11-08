using MediatR;
using MultitenantPerDb.Modules.Products.Application.Services;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand
/// Uses IProductService for business logic
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _productService.DeleteProductAsync(request.Id, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
