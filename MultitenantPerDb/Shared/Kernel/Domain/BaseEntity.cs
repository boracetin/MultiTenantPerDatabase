namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Base entity class for all domain entities
/// </summary>
public abstract class BaseEntity: SoftDeleteAudited
{
    public int Id { get; protected set; }
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
