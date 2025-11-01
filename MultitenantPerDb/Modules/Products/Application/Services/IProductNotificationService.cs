using MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

namespace MultitenantPerDb.Modules.Products.Application.Services;

/// <summary>
/// Product Notification Service - Application Layer
/// Domain-specific notification logic using generic infrastructure services
/// </summary>
public interface IProductNotificationService
{
    Task SendProductCreatedNotificationAsync(int productId, string productName);
    Task SendLowStockAlertAsync(int productId, string productName, int currentStock);
    Task SendPriceChangedNotificationAsync(int productId, string productName, decimal oldPrice, decimal newPrice);
}

/// <summary>
/// Product Notification Service Implementation
/// </summary>
public class ProductNotificationService : IProductNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductNotificationService> _logger;

    public ProductNotificationService(
        IEmailService emailService,
        ISmsService smsService,
        IConfiguration configuration,
        ILogger<ProductNotificationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendProductCreatedNotificationAsync(int productId, string productName)
    {
        try
        {
            var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
            
            var subject = "New Product Created";
            var body = $@"
                <h2>New Product Added</h2>
                <p><strong>Product ID:</strong> {productId}</p>
                <p><strong>Product Name:</strong> {productName}</p>
                <p>This is an automated notification.</p>
            ";

            await _emailService.SendEmailAsync(adminEmail, subject, body);
            
            _logger.LogInformation("Product created notification sent: {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send product created notification: {ProductId}", productId);
        }
    }

    public async Task SendLowStockAlertAsync(int productId, string productName, int currentStock)
    {
        try
        {
            var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
            var adminPhone = _configuration["AdminPhone"];
            
            // Email notification
            var subject = $"Low Stock Alert - {productName}";
            var body = $@"
                <h2>⚠️ Low Stock Warning</h2>
                <p><strong>Product:</strong> {productName} (ID: {productId})</p>
                <p><strong>Current Stock:</strong> {currentStock}</p>
                <p style='color: red;'>Please restock this item soon!</p>
            ";

            await _emailService.SendEmailAsync(adminEmail, subject, body);

            // SMS notification if phone number is configured
            if (!string.IsNullOrEmpty(adminPhone))
            {
                var smsMessage = $"Low Stock Alert: {productName} has only {currentStock} items left. Please restock.";
                await _smsService.SendSmsAsync(adminPhone, smsMessage);
            }
            
            _logger.LogInformation("Low stock alert sent: {ProductId} - Stock: {Stock}", productId, currentStock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send low stock alert: {ProductId}", productId);
        }
    }

    public async Task SendPriceChangedNotificationAsync(int productId, string productName, decimal oldPrice, decimal newPrice)
    {
        try
        {
            var adminEmail = _configuration["AdminEmail"] ?? "admin@example.com";
            
            var priceChange = newPrice - oldPrice;
            var changeType = priceChange > 0 ? "increased" : "decreased";
            var changePercentage = Math.Abs((priceChange / oldPrice) * 100);
            
            var subject = $"Price Changed - {productName}";
            var body = $@"
                <h2>Price Update Notification</h2>
                <p><strong>Product:</strong> {productName} (ID: {productId})</p>
                <p><strong>Old Price:</strong> ${oldPrice:F2}</p>
                <p><strong>New Price:</strong> ${newPrice:F2}</p>
                <p><strong>Change:</strong> {changeType} by {changePercentage:F1}%</p>
            ";

            await _emailService.SendEmailAsync(adminEmail, subject, body);
            
            _logger.LogInformation("Price change notification sent: {ProductId} - {OldPrice} -> {NewPrice}", 
                productId, oldPrice, newPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send price change notification: {ProductId}", productId);
        }
    }
}
