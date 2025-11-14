using Microsoft.AspNetCore.SignalR;
using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Core.Infrastructure.Hubs;

/// <summary>
/// Hub notification service implementation
/// Uses generic methods for type-safe entity operations
/// </summary>
public class HubNotificationService : IHubNotificationService
{
    protected readonly IHubContext<NotificationHub> _hubContext;
    protected readonly ICurrentUserService _currentUserService;
    protected readonly IAppLogger<HubNotificationService> _logger;

    public HubNotificationService(
        IHubContext<NotificationHub> hubContext,
        ICurrentUserService currentUserService,
        Application.Interfaces.ILoggerFactory loggerFactory)
    {
        _hubContext = hubContext;
        _currentUserService = currentUserService;
        _logger = loggerFactory.CreateLogger<HubNotificationService>();
    }

    public virtual async Task NotifyCreatedAsync<TEntity>(TEntity entity, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityName = typeof(TEntity).Name;
        var eventName = $"{entityName}Created";
        var notification = CreateNotification(entityName, "Created", entity, tenantId);

        await BroadcastToTenantAsync(eventName, notification, cancellationToken);
        await SendToEntityTypeAsync<TEntity>(eventName, notification, cancellationToken);
        
        _logger.LogInformation("{EntityName} created notification sent", entityName);
    }

    public virtual async Task NotifyUpdatedAsync<TEntity>(TEntity entity, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityName = typeof(TEntity).Name;
        var eventName = $"{entityName}Updated";
        var notification = CreateNotification(entityName, "Updated", entity, tenantId);

        await BroadcastToTenantAsync(eventName, notification, cancellationToken);
        await SendToEntityTypeAsync<TEntity>(eventName, notification, cancellationToken);
        
        _logger.LogInformation("{EntityName} updated notification sent", entityName);
    }

    public virtual async Task NotifyDeletedAsync<TEntity>(object entityId, string? tenantId = null, CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityName = typeof(TEntity).Name;
        var eventName = $"{entityName}Deleted";
        var notification = CreateNotification(entityName, "Deleted", new { Id = entityId }, tenantId);

        await BroadcastToTenantAsync(eventName, notification, cancellationToken);
        await SendToEntityTypeAsync<TEntity>(eventName, notification, cancellationToken);
        
        _logger.LogInformation("{EntityName} deleted notification sent - Id: {EntityId}", entityName, entityId);
    }

    public virtual async Task SendNotificationAsync(string eventName, object data, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? _currentUserService.TenantId;
        
        if (!string.IsNullOrEmpty(tenant))
        {
            await _hubContext.Clients.Group($"Tenant_{tenant}").SendAsync(eventName, data, cancellationToken);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync(eventName, data, cancellationToken);
        }
        
        _logger.LogInformation("Custom notification sent: {EventName}", eventName);
    }

    public virtual async Task SendToUserAsync(string userId, string eventName, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId).SendAsync(eventName, data, cancellationToken);
        _logger.LogDebug("Notification sent to user: {UserId} - Event: {EventName}", userId, eventName);
    }

    public virtual async Task SendToGroupAsync(string groupName, string eventName, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(eventName, data, cancellationToken);
        _logger.LogDebug("Notification sent to group: {GroupName} - Event: {EventName}", groupName, eventName);
    }

    public virtual async Task SendToEntityInstanceAsync<TEntity>(object entityId, string eventName, object data, CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityName = typeof(TEntity).Name;
        await _hubContext.Clients.Group($"{entityName}_{entityId}").SendAsync(eventName, data, cancellationToken);
        _logger.LogDebug("Notification sent to {EntityName} instance: {EntityId}", entityName, entityId);
    }

    public virtual async Task SendToEntityTypeAsync<TEntity>(string eventName, object data, CancellationToken cancellationToken = default) where TEntity : class
    {
        var entityName = typeof(TEntity).Name;
        await _hubContext.Clients.Group($"Entity_{entityName}").SendAsync(eventName, data, cancellationToken);
    }

    protected virtual async Task BroadcastToTenantAsync(string methodName, object data, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            await _hubContext.Clients.Group($"Tenant_{tenantId}").SendAsync(methodName, data, cancellationToken);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync(methodName, data, cancellationToken);
        }
    }

    protected virtual object CreateNotification(string entityName, string eventType, object data, string? tenantId)
    {
        return new
        {
            EntityName = entityName,
            EventType = eventType,
            Data = data,
            TenantId = tenantId ?? _currentUserService.TenantId,
            UserId = _currentUserService.UserId,
            Timestamp = DateTime.UtcNow
        };
    }
}
