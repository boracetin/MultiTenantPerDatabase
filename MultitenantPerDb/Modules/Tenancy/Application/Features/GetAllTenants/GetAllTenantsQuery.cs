using MediatR;
using MultitenantPerDb.Modules.Tenancy.Application.DTOs;

namespace MultitenantPerDb.Modules.Tenancy.Application.Features.GetAllTenants;

/// <summary>
/// Query to get all tenants
/// Read-only operation - no transaction needed
/// </summary>
public record GetAllTenantsQuery(bool? ActiveOnly = null) : IRequest<IEnumerable<TenantDto>>;
