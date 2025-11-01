using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure;

/// <summary>
/// Background job service interface
/// </summary>
public interface IBackgroundJobService
{
    TenantContext GetCurrentTenantContext();
    Task<T> ExecuteJobAsync<T>(string tenantId, Func<IServiceProvider, Task<T>> jobFunction);
    Task ExecuteJobAsync(string tenantId, Func<IServiceProvider, Task> jobFunction);
}
