using MultitenantPerDb.Infrastructure.Services;

namespace MultitenantPerDb.Infrastructure.Middleware;

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
        // 1. User Claim'den TenantId (Authentication varsa) - TenantResolver içinde otomatik çalışır
        // 2. HTTP Header'dan TenantId (X-Tenant-ID)
        // 3. Query string'den TenantId

        // User authenticated ise TenantId claim'i TenantResolver içinde otomatik çözülür
        // Manuel set etmeye gerek yok - TenantResolver.TenantId getter'ı zaten claim'e bakıyor
        
        // Eğer authenticated değilse, header veya query string'den al
        if (context.User?.Identity?.IsAuthenticated != true)
        {
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
