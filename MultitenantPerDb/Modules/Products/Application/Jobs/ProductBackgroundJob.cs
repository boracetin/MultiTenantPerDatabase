using MultitenantPerDb.Modules.Tenancy.Infrastructure.Services;
using MultitenantPerDb.Modules.Products.Application.Services;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Modules.Tenancy.Infrastructure.Persistence;
using MultitenantPerDb.Core.Infrastructure;

namespace MultitenantPerDb.Modules.Products.Application.Jobs;

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
            var productService = serviceProvider.GetRequiredService<IProductService>();

            var product = await productService.GetByIdAsync(productId);
            if (product != null)
            {
                // ProductService handles business logic and validation
                await productService.UpdateStockAsync(productId, newStock - product.Stock);

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
        var mainDbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
        var tenants = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(mainDbContext.Tenants.Where(t => t.IsActive));

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
            var productService = serviceProvider.GetRequiredService<IProductService>();

            var lowStockProducts = await productService.GetLowStockProductsAsync(threshold: 10);
            
            if (lowStockProducts.Any())
            {
                _logger.LogWarning("Düşük stoklu ürünler bulundu: TenantId={TenantId}, Count={Count}", 
                    tenantId, lowStockProducts.Count());
                
                // Burada email gönderme, notification, vb. işlemler yapılabilir
            }
        });
    }
}

