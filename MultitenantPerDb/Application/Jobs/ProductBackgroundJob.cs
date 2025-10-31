using MultitenantPerDb.Infrastructure.Services;
using MultitenantPerDb.Domain.Repositories;
using MultitenantPerDb.Infrastructure.Persistence;
using MultitenantPerDb.Application.Services;

namespace MultitenantPerDb.Application.Jobs;

/// <summary>
/// Product background job - Application layer
/// Hangfire veya diğer background job framework'leri ile kullanılabilir
/// </summary>
public class ProductBackgroundJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProductBackgroundJob> _logger;

    public ProductBackgroundJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProductBackgroundJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Background job - TenantId ile çalışır (Generic wrapper kullanır)
    /// Hangfire: BackgroundJob.Enqueue(() => UpdateProductStockAsync(tenantId, productId, newStock));
    /// </summary>
    public async Task UpdateProductStockAsync(string tenantId, int productId, int newStock)
    {
        _logger.LogInformation("Background job başladı: TenantId={TenantId}, ProductId={ProductId}", tenantId, productId);

        // BackgroundJobService ile generic wrapper kullan
        using var scope = _serviceScopeFactory.CreateScope();
        var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

        await backgroundJobService.ExecuteJobAsync(tenantId, async (serviceProvider) =>
        {
            // Scope ve TenantId otomatik yönetiliyor - sadece iş mantığını yaz
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.GetRepository<ProductRepository>();

            var product = await repository.GetByIdAsync(productId);
            if (product != null)
            {
                // DDD business method kullan
                product.UpdateStock(newStock);
                repository.Update(product);
                await unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Ürün stoku güncellendi: ProductId={ProductId}, NewStock={NewStock}", productId, newStock);
            }
        });
    }

    /// <summary>
    /// Scheduled job örneği - Her tenant için stok kontrolü
    /// Hangfire: RecurringJob.AddOrUpdate(() => CheckLowStockForAllTenantsAsync(), Cron.Daily);
    /// </summary>
    public async Task CheckLowStockForAllTenantsAsync()
    {
        _logger.LogInformation("Tüm tenantlar için stok kontrolü başladı");

        using var scope = _serviceScopeFactory.CreateScope();
        
        // Tenant listesini al
        var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        var tenants = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(tenantDbContext.Tenants.Where(t => t.IsActive));

        foreach (var tenant in tenants)
        {
            try
            {
                await CheckLowStockForTenantAsync(tenant.Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tenant için stok kontrolü başarısız: TenantId={TenantId}", tenant.Id);
            }
        }

        _logger.LogInformation("Tüm tenantlar için stok kontrolü tamamlandı");
    }

    private async Task CheckLowStockForTenantAsync(string tenantId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();

        await backgroundJobService.ExecuteJobAsync(tenantId, async (serviceProvider) =>
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.GetRepository<ProductRepository>();

            var lowStockProducts = await repository.FindAsync(p => p.Stock < 10);
            
            if (lowStockProducts.Any())
            {
                _logger.LogWarning("Düşük stoklu ürünler bulundu: TenantId={TenantId}, Count={Count}", 
                    tenantId, lowStockProducts.Count());
                
                // Burada email gönderme, notification, vb. işlemler yapılabilir
            }
        });
    }
}
