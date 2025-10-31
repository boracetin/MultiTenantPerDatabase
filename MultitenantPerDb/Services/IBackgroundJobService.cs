namespace MultitenantPerDb.Services;

public interface IBackgroundJobService
{
    /// <summary>
    /// Mevcut HTTP request'ten TenantContext oluşturur (Job kuyruğa eklerken kullan)
    /// </summary>
    TenantContext GetCurrentTenantContext();
    
    /// <summary>
    /// Background job'ı TenantId ile çalıştırır - Scope ve Context otomatik yönetilir
    /// </summary>
    Task ExecuteJobAsync(string tenantId, Func<IServiceProvider, Task> job);
    
    /// <summary>
    /// Background job'ı TenantId ile çalıştırır ve sonuç döner - Scope ve Context otomatik yönetilir
    /// </summary>
    Task<T> ExecuteJobAsync<T>(string tenantId, Func<IServiceProvider, Task<T>> job);
}
