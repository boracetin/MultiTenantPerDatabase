namespace MultitenantPerDb.Core.Domain.Constants;

/// <summary>
/// Entity names for SignalR notifications
/// Centralized constants to avoid magic strings
/// </summary>
public static class EntityNames
{
    public const string Product = "Product";
    public const string User = "User";
    public const string Tenant = "Tenant";
    public const string Order = "Order";
    public const string Customer = "Customer";
}
