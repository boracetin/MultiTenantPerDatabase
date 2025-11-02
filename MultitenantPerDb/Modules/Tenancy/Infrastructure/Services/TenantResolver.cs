using System.Security.Claims;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

/// <summary>
/// Resolves tenant ID from multiple sources with priority:
/// 1. Explicit set (background jobs)
/// 2. User claims (JWT - authenticated requests)
/// 3. Subdomain (e.g., tenant1.myapp.com)
/// 4. HTTP Header (X-Tenant-ID)
/// 5. Query string (tenantId)
/// </summary>
public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _tenantId;
    private bool _isExplicitlySet = false;

    public TenantResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId
    {
        get
        {
            // 1. Manuel set edilmişse (background işlem için) onu kullan
            if (_isExplicitlySet && !string.IsNullOrEmpty(_tenantId))
            {
                return _tenantId;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return _tenantId;
            }

            // 2. User authenticated ise JWT claim'den TenantId'yi al
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = httpContext.User.FindFirst("TenantId");
                if (tenantClaim != null)
                {
                    return tenantClaim.Value;
                }
            }

            // 3. Subdomain'den tenant bilgisini çıkar
            var subdomainTenant = ExtractTenantFromSubdomain(httpContext);
            if (!string.IsNullOrEmpty(subdomainTenant))
            {
                return subdomainTenant;
            }

            // 4. Fallback: Manuel set edilmiş _tenantId (middleware tarafından set edilmiş olabilir)
            return _tenantId;
        }
    }

    /// <summary>
    /// Extracts tenant identifier from subdomain
    /// Examples: 
    /// - tenant1.localhost:5231 -> tenant1
    /// - tenant2.myapp.com -> tenant2
    /// - www.myapp.com -> null (reserved subdomain)
    /// - api.myapp.com -> null (reserved subdomain)
    /// </summary>
    private string? ExtractTenantFromSubdomain(HttpContext httpContext)
    {
        var host = httpContext.Request.Host.Host;

        // Reserved subdomains that should not be treated as tenant identifiers
        var reservedSubdomains = new[] { "www", "api", "admin", "app", "localhost" };

        // Split host by dots
        var parts = host.Split('.');

        // If only one part (e.g., "localhost") or two parts (e.g., "myapp.com"), no subdomain
        if (parts.Length <= 2)
        {
            return null;
        }

        // First part is the subdomain
        var subdomain = parts[0].ToLowerInvariant();

        // Check if subdomain is reserved
        if (reservedSubdomains.Contains(subdomain))
        {
            return null;
        }

        return subdomain;
    }

    public void SetTenant(string tenantId)
    {
        _tenantId = tenantId;
        _isExplicitlySet = true;
    }

    public void ClearTenant()
    {
        _tenantId = null;
        _isExplicitlySet = false;
    }

    public bool HasTenant()
    {
        return !string.IsNullOrEmpty(TenantId);
    }
}
