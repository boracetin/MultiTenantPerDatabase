using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Core.Infrastructure.Tenancy.Contract;
using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Core.Application.Abstractions;

namespace MultitenantPerDb.Modules.Products.Application.Features.CreateProduct;

/// <summary>
/// Command to create a new product
/// Uses ApplicationDbContext for tenant-specific data
/// Implements IAuthorizedRequest for authorization check (requires "products:write" permission)
/// Implements IRateLimitedRequest for rate limiting (100 requests per minute per user per tenant)
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>, 
    IRateLimitedRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }


    // Rate limiting configuration
    int IRateLimitedRequest.Limit => 100; // 100 requests
    int IRateLimitedRequest.WindowSeconds => 60; // per 60 seconds (1 minute)
    RateLimitScope IRateLimitedRequest.Scope => RateLimitScope.PerUserPerTenant; // Per user in each tenant
}
