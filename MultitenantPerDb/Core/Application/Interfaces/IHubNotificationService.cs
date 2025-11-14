namespace MultitenantPerDb.Core.Application.Interfaces;

/// <summary>
/// Notification service for SignalR Hub operations
/// Uses generic methods for type-safe entity operations
/// </summary>
public interface IHubNotificationService
{
    /// <summary>
    /// Notify clients about entity creation
    /// </summary>
    Task NotifyCreatedAsync<TEntity>(TEntity entity, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Notify clients about entity update
    /// </summary>
    Task NotifyUpdatedAsync<TEntity>(TEntity entity, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Notify clients about entity deletion
    /// </summary>
    Task NotifyDeletedAsync<TEntity>(object entityId, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Send custom notification to all clients
    /// </summary>
    Task SendNotificationAsync(string eventName, object data, string? tenantId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification to specific user
    /// </summary>
    Task SendToUserAsync(string userId, string eventName, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification to specific group
    /// </summary>
    Task SendToGroupAsync(string groupName, string eventName, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send notification to specific entity instance subscribers
    /// </summary>
    Task SendToEntityInstanceAsync<TEntity>(object entityId, string eventName, object data, CancellationToken cancellationToken = default) where TEntity : class;
    
    /// <summary>
    /// Send notification to entity type subscribers
    /// </summary>
    Task SendToEntityTypeAsync<TEntity>(string eventName, object data, CancellationToken cancellationToken = default) where TEntity : class;
}
