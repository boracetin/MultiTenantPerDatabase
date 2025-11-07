using MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Background job service for Hangfire and background tasks
/// </summary>
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

    public TenantContext GetCurrentTenantContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext mevcut değil");
        }

        var tenantId = httpContext.User.FindFirst("TenantId")?.Value;
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException("TenantId claim bulunamadı");
        }

        return new TenantContext
        {
            TenantId = tenantId,
            UserId = userId,
        };
    }

    public async Task<T> ExecuteJobAsync<T>(string tenantId, Func<IServiceProvider, Task<T>> jobFunction)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();

        try
        {
            tenantResolver.SetTenant(tenantId);
            return await jobFunction(scope.ServiceProvider);
        }
        finally
        {
            tenantResolver.ClearTenant();
        }
    }

    public async Task ExecuteJobAsync(string tenantId, Func<IServiceProvider, Task> jobFunction)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();

        try
        {
            tenantResolver.SetTenant(tenantId);
            await jobFunction(scope.ServiceProvider);
        }
        finally
        {
            tenantResolver.ClearTenant();
        }
    }
}
