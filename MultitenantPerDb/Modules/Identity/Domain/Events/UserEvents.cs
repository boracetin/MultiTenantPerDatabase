using MultitenantPerDb.Shared.Kernel.Domain;
using MultitenantPerDb.Modules.Identity.Domain.Entities;

namespace MultitenantPerDb.Modules.Identity.Domain.Events;

public class UserLoggedInEvent : IDomainEvent
{
    public User User { get; }
    public DateTime OccurredOn { get; }

    public UserLoggedInEvent(User user)
    {
        User = user;
        OccurredOn = DateTime.UtcNow;
    }
}
