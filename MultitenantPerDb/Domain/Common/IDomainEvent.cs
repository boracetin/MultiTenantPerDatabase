namespace MultitenantPerDb.Domain.Common;

/// <summary>
/// Interface for all domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
