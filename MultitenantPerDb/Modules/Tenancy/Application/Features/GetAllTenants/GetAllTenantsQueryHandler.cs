using MediatR;
using MultitenantPerDb.Modules.Tenancy.Application.DTOs;
using MultitenantPerDb.Modules.Tenancy.Application.Services;

namespace MultitenantPerDb.Modules.Tenancy.Application.Features.GetAllTenants;

/// <summary>
/// Handler for GetAllTenantsQuery
/// Uses TenantService to fetch tenant data
/// </summary>
public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, IEnumerable<TenantDto>>
{
    private readonly ITenantService _tenantService;

    public GetAllTenantsQueryHandler(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        // Get tenants based on filter
        var tenants = request.ActiveOnly == true
            ? await _tenantService.GetActiveTenantsAsync(cancellationToken)
            : await _tenantService.GetAllTenantsAsync(cancellationToken);

        // Map to DTOs
        return tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Subdomain = t.Subdomain,
            DisplayName = t.DisplayName,
            LogoUrl = t.LogoUrl,
            BackgroundImageUrl = t.BackgroundImageUrl,
            PrimaryColor = t.PrimaryColor,
            SecondaryColor = t.SecondaryColor,
            CustomCss = t.CustomCss,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        });
    }
}
