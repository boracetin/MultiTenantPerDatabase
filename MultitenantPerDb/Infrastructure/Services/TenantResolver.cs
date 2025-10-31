using System.Security.Claims;

namespace MultitenantPerDb.Infrastructure.Services;

/// <summary>
/// Resolves tenant ID from HTTP context (claims) or explicit setting (background jobs)
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
            // Manuel set edilmişse (background işlem için) onu kullan
            if (_isExplicitlySet && !string.IsNullOrEmpty(_tenantId))
            {
                return _tenantId;
            }

            // HttpContext varsa User claim'den TenantId'yi al
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = httpContext.User.FindFirst("TenantId");
                if (tenantClaim != null)
                {
                    return tenantClaim.Value;
                }
            }

            return _tenantId;
        }
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
