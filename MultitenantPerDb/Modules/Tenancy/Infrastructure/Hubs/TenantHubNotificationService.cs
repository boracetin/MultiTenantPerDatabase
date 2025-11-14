using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Modules.Tenancy.Domain.Entities;

namespace MultitenantPerDb.Modules.Tenancy.Infrastructure.Hubs;

public class TenantHubNotificationService
{
    private readonly IHubNotificationService _hubNotification;

    public TenantHubNotificationService(IHubNotificationService hubNotification)
    {
        _hubNotification = hubNotification;
    }

    public async Task NotifyCreatedAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyCreatedAsync(
            tenant,
            cancellationToken: cancellationToken);
            
        // Also notify system admins
        await _hubNotification.SendToGroupAsync(
            "SystemAdmins",
            "NewTenantRegistered",
            new 
            { 
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                Subdomain = tenant.Subdomain,
                RegisteredAt = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task NotifyUpdatedAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyUpdatedAsync(
            tenant,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyDeletedAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyDeletedAsync<Tenant>(
            tenantId,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyTenantStatusChangedAsync(Tenant tenant, bool isActive, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendToGroupAsync(
            $"TenantEvents_{tenant.Id}",
            "TenantStatusChanged",
            new 
            { 
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                IsActive = isActive,
                Message = isActive ? "Tenant activated" : "Tenant deactivated"
            },
            cancellationToken);
    }

    public async Task NotifySubscriptionExpiringAsync(Tenant tenant, int daysRemaining, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendToGroupAsync(
            $"Tenant_{tenant.Id}",
            "SubscriptionExpiring",
            new 
            { 
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                DaysRemaining = daysRemaining,
                Message = $"Your subscription will expire in {daysRemaining} days"
            },
            cancellationToken);
    }
}
