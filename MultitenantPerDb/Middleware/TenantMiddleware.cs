using MultitenantPerDb.Services;

namespace MultitenantPerDb.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        // Öncelik sırası:
        // 1. User Claim'den TenantId (Authentication varsa)
        // 2. HTTP Header'dan TenantId (X-Tenant-ID)
        // 3. Query string'den TenantId

        // User authenticated ise ve TenantId claim'i varsa otomatik resolve olur (TenantResolver içinde)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("TenantId");
            if (tenantClaim != null)
            {
                tenantResolver.SetTenant(tenantClaim.Value);
                await _next(context);
                return;
            }
        }

        // HTTP Header'dan TenantId'yi al (Authentication olmayan istekler için)
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            tenantResolver.SetTenant(tenantId.ToString());
        }
        // Query string'den de alınabilir (opsiyonel)
        else if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
        {
            tenantResolver.SetTenant(queryTenantId.ToString());
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
