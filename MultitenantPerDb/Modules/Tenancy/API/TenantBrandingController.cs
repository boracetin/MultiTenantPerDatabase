using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Tenancy.API;

/// <summary>
/// Tenant branding and customization API
/// Returns UI customization settings based on subdomain
/// Does NOT provide data access - only branding information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantBrandingController : ControllerBase
{
    private readonly MainDbContext _mainDbContext;
    private readonly ITenantResolver _tenantResolver;

    public TenantBrandingController(MainDbContext mainDbContext, ITenantResolver tenantResolver)
    {
        _mainDbContext = mainDbContext;
        _tenantResolver = tenantResolver;
    }

    /// <summary>
    /// Get branding settings for current subdomain
    /// Used by frontend to customize UI (logo, colors, background, etc.)
    /// Anonymous access allowed - only returns branding info, no sensitive data
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentBranding()
    {
        // Get subdomain from request (for UI customization only)
        var subdomain = _tenantResolver.GetSubdomainForBranding();

        if (string.IsNullOrEmpty(subdomain))
        {
            return Ok(new
            {
                message = "No subdomain detected. Using default branding.",
                branding = GetDefaultBranding()
            });
        }

        // Find tenant by subdomain
        var tenant = await _mainDbContext.Tenants
            .Where(t => t.Subdomain == subdomain && t.IsActive)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Subdomain,
                t.DisplayName,
                t.LogoUrl,
                t.BackgroundImageUrl,
                t.PrimaryColor,
                t.SecondaryColor,
                t.CustomCss
            })
            .FirstOrDefaultAsync();

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

    /// <summary>
    /// Get branding settings by subdomain (explicit)
    /// </summary>
    [HttpGet("by-subdomain/{subdomain}")]
    public async Task<IActionResult> GetBrandingBySubdomain(string subdomain)
    {
        var tenant = await _mainDbContext.Tenants
            .Where(t => t.Subdomain == subdomain.ToLower() && t.IsActive)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Subdomain,
                t.DisplayName,
                t.LogoUrl,
                t.BackgroundImageUrl,
                t.PrimaryColor,
                t.SecondaryColor,
                t.CustomCss
            })
            .FirstOrDefaultAsync();

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
