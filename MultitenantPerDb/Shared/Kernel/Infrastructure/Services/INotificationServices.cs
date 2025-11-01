namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Services;

/// <summary>
/// Infrastructure Service - Email notifications
/// External service dependency, implementation infrastructure layer'da
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendProductCreatedNotificationAsync(int productId, string productName);
}

/// <summary>
/// Infrastructure Service - SMS notifications
/// </summary>
public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
}

/// <summary>
/// Infrastructure Service - External API integration
/// </summary>
public interface IInventoryApiService
{
    Task SyncInventoryAsync(int productId, int stock);
    Task<decimal> GetMarketPriceAsync(string productName);
}
