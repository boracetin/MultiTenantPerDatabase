namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Interface for all domain events
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
