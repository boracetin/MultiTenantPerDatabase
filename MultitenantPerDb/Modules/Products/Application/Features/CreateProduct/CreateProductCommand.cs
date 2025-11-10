using MediatR;
using MultitenantPerDb.Modules.Products.Application.DTOs;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Modules.Products.Application.Features.CreateProduct;

/// <summary>
/// Command to create a new product
/// Uses ApplicationDbContext for tenant-specific data
/// Implements IAuthorizedRequest for authorization check (requires "products:write" permission)
/// Implements IRateLimitedRequest for rate limiting (100 requests per minute per user per tenant)
/// </summary>
public record CreateProductCommand : IRequest<ProductDto>, 
    IWithoutTransactional,
    IAuthorizedRequest,
    IRateLimitedRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }

    // Authorization configuration
    string[]? IAuthorizedRequest.RequiredPermissions => new[] { "products:write" };
    string[]? IAuthorizedRequest.RequiredRoles => null; // No specific role required, permission is enough
    bool IAuthorizedRequest.RequireTenantIsolation => true; // Enforce tenant isolation

    // Rate limiting configuration
    int IRateLimitedRequest.Limit => 100; // 100 requests
    int IRateLimitedRequest.WindowSeconds => 60; // per 60 seconds (1 minute)
    RateLimitScope IRateLimitedRequest.Scope => RateLimitScope.PerUserPerTenant; // Per user in each tenant
}
