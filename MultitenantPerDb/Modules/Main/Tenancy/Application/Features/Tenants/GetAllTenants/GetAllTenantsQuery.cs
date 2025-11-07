using MediatR;
using MultitenantPerDb.Modules.Main.Tenancy.Application.DTOs;

namespace MultitenantPerDb.Modules.Main.Tenancy.Application.Features.Tenants.GetAllTenants;

/// <summary>
/// Query to get all tenants
/// Read-only operation - no transaction needed
/// </summary>
public record GetAllTenantsQuery(bool? ActiveOnly = null) : IRequest<IEnumerable<TenantDto>>;
