using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Middleware;

/// <summary>
/// Middleware for tenant identification from multiple sources
/// Priority Order:
/// 1. JWT Claims (authenticated requests) - Handled in TenantResolver
/// 2. Subdomain (e.g., tenant1.myapp.com) - Handled in TenantResolver
/// 3. HTTP Header (X-Tenant-ID) - Handled here
/// 4. Query String (tenantId) - Handled here
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        // TenantResolver automatically tries:
        // 1. JWT Claims (if authenticated)
        // 2. Subdomain extraction
        // So we only need to handle Header and Query String as fallback

        // Only set tenant explicitly if not authenticated and subdomain extraction failed
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            // Check if subdomain already resolved the tenant
            var currentTenant = tenantResolver.TenantId;
            
            if (string.IsNullOrEmpty(currentTenant))
            {
                // Fallback 1: HTTP Header (X-Tenant-ID)
                if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
                {
                    tenantResolver.SetTenant(tenantId.ToString());
                }
                // Fallback 2: Query string (tenantId)
                else if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
                {
                    tenantResolver.SetTenant(queryTenantId.ToString());
                }
            }
        }

        await _next(context);
    }
}

// Extension method for middleware registration
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolver(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
