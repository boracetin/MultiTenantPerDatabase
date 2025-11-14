using MultitenantPerDb.Core.Application.Interfaces;

namespace MultitenantPerDb.Modules.User.Infrastructure.Hubs;

public class UserHubNotificationService
{
    private readonly IHubNotificationService _hubNotification;

    public UserHubNotificationService(IHubNotificationService hubNotification)
    {
        _hubNotification = hubNotification;
    }

    public async Task NotifyCreatedAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyCreatedAsync(
            user,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyUpdatedAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyUpdatedAsync(
            user,
            cancellationToken: cancellationToken);
            
        await _hubNotification.SendToEntityInstanceAsync<Domain.Entities.User>(
            user.Id,
            "UserProfileUpdated",
            new { Id = user.Id, Username = user.Username, Email = user.Email, IsActive = user.IsActive },
            cancellationToken);
    }

    public async Task NotifyDeletedAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _hubNotification.NotifyDeletedAsync<Domain.Entities.User>(
            userId,
            cancellationToken: cancellationToken);
    }

    public async Task NotifyRoleChangedAsync(Domain.Entities.User user, string newRole, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendToUserAsync(
            user.Id.ToString(),
            "RoleChanged",
            new 
            { 
                UserId = user.Id,
                Email = user.Email,
                NewRole = newRole
            },
            cancellationToken);
    }

    public async Task NotifyStatusChangedAsync(Domain.Entities.User user, bool isActive, CancellationToken cancellationToken = default)
    {
        await _hubNotification.SendToUserAsync(
            user.Id.ToString(),
            "AccountStatusChanged",
            new 
            { 
                UserId = user.Id,
                Email = user.Email,
                IsActive = isActive,
                Message = isActive ? "Your account has been activated" : "Your account has been deactivated"
            },
            cancellationToken);
    }
}
