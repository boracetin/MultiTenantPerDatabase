using MediatR;
using Microsoft.AspNetCore.Mvc;
using MultitenantPerDb.Modules.Tenancy.Application.Features.GetAllTenants;
using MultitenantPerDb.Modules.Tenancy.Application.Services;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Tenancy.API;

/// <summary>
/// Tenant branding and customization API
/// Returns UI customization settings based on subdomain
/// Uses CQRS pattern for data access
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantBrandingController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IMediator _mediator;

    public TenantBrandingController(
        ITenantService tenantService,
        IMediator mediator)
    {
        _tenantService = tenantService;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all tenants
    /// CQRS Query endpoint
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllTenants([FromQuery] bool? activeOnly = null)
    {
        var query = new GetAllTenantsQuery(activeOnly);
        var tenants = await _mediator.Send(query);
        return Ok(tenants);
    }

    /// <summary>
    /// Get branding settings by subdomain (explicit)
    /// </summary>
    [HttpGet("by-subdomain/{subdomain}")]
    public async Task<IActionResult> GetBrandingBySubdomain(string subdomain)
    {
        var tenant = await _tenantService.GetBySubdomainAsync(subdomain.ToLower());

        if (tenant == null)
        {
            return NotFound(new
            {
                message = $"Tenant not found for subdomain: {subdomain}",
                subdomain
            });
        }

        return Ok(new
        {
            subdomain = tenant.Subdomain,
            branding = new
            {
                displayName = tenant.DisplayName ?? tenant.Name,
                logoUrl = tenant.LogoUrl,
                backgroundImageUrl = tenant.BackgroundImageUrl,
                primaryColor = tenant.PrimaryColor ?? "#1976D2",
                secondaryColor = tenant.SecondaryColor ?? "#424242",
                customCss = tenant.CustomCss
            }
        });
    }

    private object GetDefaultBranding()
    {
        return new
        {
            displayName = "MultiTenant App",
            logoUrl = (string?)null,
            backgroundImageUrl = (string?)null,
            primaryColor = "#1976D2",
            secondaryColor = "#424242",
            customCss = (string?)null
        };
    }
}
