using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Middleware;

/// <summary>
/// Middleware for tenant identification
/// SECURITY: TenantId comes ONLY from JWT claims (after authentication)
/// Subdomain is used ONLY for UI branding/customization, NOT for tenant identification
/// 
/// Flow:
/// 1. User navigates to tenant1.myapp.com
/// 2. Login endpoint uses subdomain to show custom branding (logo, colors, etc.)
/// 3. After successful login, JWT contains TenantId claim
/// 4. All subsequent requests use JWT TenantId for data access
/// 5. Subdomain continues to provide UI customization
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
        // TenantResolver automatically resolves TenantId from:
        // 1. Explicit set (background jobs)
        // 2. JWT Claims (authenticated requests)
        
        // NO manual setting from headers/query strings - Security risk!
        // Subdomain is available via GetSubdomainForBranding() for UI customization only
        
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
