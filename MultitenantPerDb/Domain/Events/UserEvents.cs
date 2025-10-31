using MultitenantPerDb.Domain.Common;
using MultitenantPerDb.Domain.Entities;

namespace MultitenantPerDb.Domain.Events;

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
