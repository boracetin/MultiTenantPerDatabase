namespace MultitenantPerDb.Modules.Application.Products.Application.Services;

/// <summary>
/// Application Service - Orchestrates complex workflows
/// Handler'lardan çağrılır, birden fazla repository veya external service ile çalışabilir
/// </summary>
public interface IProductOrchestrationService
{
    /// <summary>
    /// Creates product with additional operations (notifications, analytics, etc.)
    /// </summary>
    Task<int> CreateProductWithNotificationAsync(string name, string description, decimal price, int stock);

    /// <summary>
    /// Checks if product name is unique across all tenants (business rule)
    /// </summary>
    Task<bool> IsProductNameUniqueAsync(string name);
}
