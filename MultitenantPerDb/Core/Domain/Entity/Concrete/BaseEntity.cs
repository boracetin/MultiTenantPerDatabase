namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Base entity class for all domain entities with generic Id type
/// </summary>
/// <typeparam name="T">Type of the entity identifier (int, Guid, string, etc.)</typeparam>
public abstract class BaseEntity<T> : SoftDeleteAudited, IEntity<T> where T : IEquatable<T>
{
    public T Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetCreatedAt()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
