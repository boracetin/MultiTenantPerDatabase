using System.Security.Claims;

namespace MultitenantPerDb.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundJobService(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Mevcut request'ten TenantContext oluşturur (Hangfire job kuyruğa eklerken kullan)
    /// </summary>
    public TenantContext GetCurrentTenantContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return new TenantContext
            {
                TenantId = httpContext.User.FindFirst("TenantId")?.Value ?? string.Empty,
                UserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value
            };
        }

        throw new InvalidOperationException("TenantContext oluşturulamadı. Kullanıcı authenticated değil.");
    }

    /// <summary>
    /// Background job'ı TenantId ile çalıştırır - Scope ve Context otomatik yönetilir
    /// Job içinde IServiceProvider'dan istediğiniz servisi alabilirsiniz
    /// </summary>
    public async Task ExecuteJobAsync(string tenantId, Func<IServiceProvider, Task> job)
    {
        // Yeni scope oluştur
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // TenantId'yi set et
        var tenantResolver = serviceProvider.GetRequiredService<ITenantResolver>();
        tenantResolver.SetTenant(tenantId);

        try
        {
            // Job'ı çalıştır - ServiceProvider'ı parametre olarak ver
            await job(serviceProvider);
        }
        finally
        {
            tenantResolver.ClearTenant();
        }
    }

    /// <summary>
    /// Background job'ı TenantId ile çalıştırır ve sonuç döner
    /// </summary>
    public async Task<T> ExecuteJobAsync<T>(string tenantId, Func<IServiceProvider, Task<T>> job)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        var tenantResolver = serviceProvider.GetRequiredService<ITenantResolver>();
        tenantResolver.SetTenant(tenantId);

        try
        {
            return await job(serviceProvider);
        }
        finally
        {
            tenantResolver.ClearTenant();
        }
    }
}
