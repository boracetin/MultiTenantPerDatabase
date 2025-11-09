using System.Security.Claims;
using MultitenantPerDb.Core.Application.Abstractions;
using MultitenantPerDb.Core.Infrastructure.Security;

namespace MultitenantPerDb.Core.Infrastructure.Services;

/// <summary>
/// Resolves tenant ID from secure sources ONLY:
/// 1. Explicit set (background jobs) - Internal use only
/// 2. JWT Claims (authenticated requests) - ONLY secure source for TenantId
/// 
/// SECURITY: 
/// - TenantId should NEVER come from headers, query strings, or subdomains.
/// - TenantId in JWT is encrypted (AES-256) so users cannot read or modify it
/// - Decrypted automatically when resolving tenant
/// </summary>
public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEncryptionService _encryptionService;
    private string? _tenantId;
    private bool _isExplicitlySet = false;

    public TenantResolver(IHttpContextAccessor httpContextAccessor, IEncryptionService encryptionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Gets TenantId from SECURE sources only (Explicit set or JWT claims)
    /// JWT TenantId is encrypted and decrypted automatically
    /// </summary>
    public string? TenantId
    {
        get
        {
            // 1. Explicit set (background jobs, internal operations)
            if (_isExplicitlySet && !string.IsNullOrEmpty(_tenantId))
            {
                return _tenantId;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            // 2. JWT Claims (ONLY secure source for user requests)
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var encryptedTenantClaim = httpContext.User.FindFirst("TenantId");
                if (encryptedTenantClaim != null)
                {
                    try
                    {
                        // Decrypt TenantId from JWT claim
                        var decryptedTenantId = _encryptionService.Decrypt(encryptedTenantClaim.Value);
                        return decryptedTenantId;
                    }
                    catch (Exception ex)
                    {
                        // Log decryption failure (token may be tampered)
                        Console.WriteLine($"Failed to decrypt TenantId from JWT: {ex.Message}");
                        return null;
                    }
                }
            }

            // NO fallback to headers/query strings - Security risk!
            return null;
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
