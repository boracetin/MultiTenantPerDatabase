using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultitenantPerDb.Modules.Products.Application.Jobs;
using MultitenantPerDb.Modules.Products.Application.Services;
using MultitenantPerDb.Modules.Products.Infrastructure.Persistence;
using MultitenantPerDb.Core.Domain;
using MultitenantPerDb.Core.Infrastructure;

namespace MultitenantPerDb.Modules.Products.API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BackgroundJobController : ControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundJobController> _logger;

    public BackgroundJobController(
        IBackgroundJobService backgroundJobService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundJobController> logger)
    {
        _backgroundJobService = backgroundJobService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Background job başlatır - TenantId otomatik token'dan alınır
    /// </summary>
    [HttpPost("update-stock")]
    public ActionResult EnqueueStockUpdate([FromBody] StockUpdateRequest request)
    {
        try
        {
            // Mevcut request'ten TenantContext al
            var tenantContext = _backgroundJobService.GetCurrentTenantContext();

            // Background job kuyruğa ekle (Hangfire kullanıyorsanız)
            // BackgroundJob.Enqueue(() => job.UpdateProductStockAsync(tenantContext.TenantId, request.ProductId, request.NewStock));

            // Demo: Task.Run ile simüle edelim (gerçekte Hangfire kullanın)
            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<ProductBackgroundJob>();
                await job.UpdateProductStockAsync(tenantContext.TenantId, request.ProductId, request.NewStock);
            });

            return Ok(new
            {
                message = "Stok güncelleme işi kuyruğa eklendi",
                tenantId = tenantContext.TenantId,
                productId = request.ProductId,
                jobType = "UpdateStock"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Background işlemi inline çalıştırır (test amaçlı)
    /// Generic wrapper kullanımı örneği
    /// </summary>
    [HttpPost("test-background")]
    public async Task<ActionResult> TestBackgroundExecution()
    {
        try
        {
            var tenantContext = _backgroundJobService.GetCurrentTenantContext();

            // Generic wrapper ile job çalıştır - scope ve context otomatik
            await _backgroundJobService.ExecuteJobAsync(tenantContext.TenantId, async (serviceProvider) =>
            {
                // İhtiyacınız olan servisleri alın
                var logger = serviceProvider.GetRequiredService<ILogger<BackgroundJobController>>();
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork<ProductsDbContext>>();
                
                logger.LogInformation("Background işlem çalışıyor: TenantId={TenantId}", tenantContext.TenantId);
                
                // İş mantığınızı buraya yazın
                await Task.Delay(1000);
                
                logger.LogInformation("Background işlem tamamlandı");
            });

            return Ok(new { message = "Background işlem başarıyla tamamlandı" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Background job'dan sonuç döndürme örneği
    /// </summary>
    [HttpPost("calculate-total-stock")]
    public async Task<ActionResult> CalculateTotalStock()
    {
        try
        {
            var tenantContext = _backgroundJobService.GetCurrentTenantContext();

            // Generic wrapper ile sonuç döndür
            var totalStock = await _backgroundJobService.ExecuteJobAsync<int>(tenantContext.TenantId, async (serviceProvider) =>
            {
                var productService = serviceProvider.GetRequiredService<IProductService>();
                
                var pagedProducts = await productService.GetProductsPagedAsync(1, 10000);
                return pagedProducts.Items.Sum(p => p.Stock);
            });

            return Ok(new 
            { 
                message = "Toplam stok hesaplandı",
                tenantId = tenantContext.TenantId,
                totalStock = totalStock
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class StockUpdateRequest
{
    public int ProductId { get; set; }
    public int NewStock { get; set; }
}

