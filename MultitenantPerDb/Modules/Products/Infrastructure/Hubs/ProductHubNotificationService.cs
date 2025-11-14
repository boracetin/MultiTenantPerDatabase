using Microsoft.AspNetCore.SignalR;
using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Core.Infrastructure.Hubs;
using MultitenantPerDb.Modules.Products.Domain.Entities;

namespace MultitenantPerDb.Modules.Products.Infrastructure.Hubs;

/// <summary>
/// Hub notification service for Product entity
/// </summary>
public class ProductHubNotificationService
{
    private readonly IHubNotificationService _hubNotification;

    public ProductHubNotificationService(IHubNotificationService hubNotification)
    {
        _hubNotification = hubNotification;
    }

    public async Task NotifyCreatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyCreatedAsync(
            product,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyUpdatedAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyUpdatedAsync(
            product,
            cancellationToken: cancellationToken);
            
        // Also notify specific product instance subscribers
        await _hubNotification.SendToEntityInstanceAsync<Product>(
            product.Id,
            "ProductUpdated",
            new { Id = product.Id, Name = product.Name, Price = product.Price, Stock = product.Stock },
            cancellationToken);
    }

    public async Task NotifyDeletedAsync(int productId, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyDeletedAsync<Product>(
            productId,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyLowStockAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendNotificationAsync(
            "LowStockAlert",
            new 
            { 
                ProductId = product.Id,
                ProductName = product.Name,
                CurrentStock = product.Stock,
                MinimumStock = 10
            },
            cancellationToken: cancellationToken);
    }

    public async Task NotifyPriceChangedAsync(Product product, decimal oldPrice, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendNotificationAsync(
            "PriceChanged",
            new 
            { 
                ProductId = product.Id,
                ProductName = product.Name,
                OldPrice = oldPrice,
                NewPrice = product.Price,
                ChangePercentage = ((product.Price - oldPrice) / oldPrice) * 100
            },
            cancellationToken: cancellationToken);
    }
}
