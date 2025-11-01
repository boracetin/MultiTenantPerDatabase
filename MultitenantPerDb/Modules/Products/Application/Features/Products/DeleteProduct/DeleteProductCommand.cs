using MediatR;

namespace MultitenantPerDb.Modules.Products.Application.Features.Products.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(int Id) : IRequest<bool>;
