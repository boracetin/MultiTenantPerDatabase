namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Generic notification event payload
/// </summary>
public class HubNotificationEvent<TEntity> where TEntity : class
{
    public string EventType { get; set; } = string.Empty;
    public TEntity? Entity { get; set; }
    public object? EntityId { get; set; }
    public string? TenantId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Hub event types
/// </summary>
public static class HubEventTypes
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
    public const string Custom = "Custom";
}
