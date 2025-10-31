using MultitenantPerDb.Infrastructure.Services;

namespace MultitenantPerDb.Application.Services;

/// <summary>
/// Background job service interface
/// </summary>
public interface IBackgroundJobService
{
    TenantContext GetCurrentTenantContext();
    Task<T> ExecuteJobAsync<T>(string tenantId, Func<IServiceProvider, Task<T>> jobFunction);
    Task ExecuteJobAsync(string tenantId, Func<IServiceProvider, Task> jobFunction);
}
