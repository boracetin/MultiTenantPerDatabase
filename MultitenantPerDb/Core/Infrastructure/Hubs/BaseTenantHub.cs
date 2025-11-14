using Microsoft.AspNetCore.SignalR;
using MultitenantPerDb.Core.Application.Abstractions;

namespace MultitenantPerDb.Core.Infrastructure.Hubs;

/// <summary>
/// Generic tenant-aware Hub for all entities
/// Single hub endpoint for all real-time notifications
/// </summary>
public class NotificationHub : Hub
{
    protected readonly ITenantResolver _tenantResolver;

    public NotificationHub(ITenantResolver tenantResolver)
    {
        _tenantResolver = tenantResolver;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = _tenantResolver.TenantId;
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            // Add connection to tenant-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
        }

        // Add to authenticated users group if user is authenticated
        if (Context.UserIdentifier != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AuthenticatedUsers");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = _tenantResolver.TenantId;
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
        }

        if (Context.UserIdentifier != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AuthenticatedUsers");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific entity type updates
    /// </summary>
    public async Task SubscribeToEntity(string entityType)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Entity_{entityType}");
    }

    /// <summary>
    /// Unsubscribe from entity type updates
    /// </summary>
    public async Task UnsubscribeFromEntity(string entityType)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Entity_{entityType}");
    }

    /// <summary>
    /// Subscribe to specific entity instance
    /// </summary>
    public async Task SubscribeToEntityInstance(string entityType, string entityId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{entityType}_{entityId}");
    }

    /// <summary>
    /// Unsubscribe from entity instance
    /// </summary>
    public async Task UnsubscribeFromEntityInstance(string entityType, string entityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{entityType}_{entityId}");
    }

    /// <summary>
    /// Join a custom group
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave a custom group
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
