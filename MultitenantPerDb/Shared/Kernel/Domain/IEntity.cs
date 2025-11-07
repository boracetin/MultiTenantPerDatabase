namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Marker interface for all entities
/// Used as generic constraint for Repository and UnitOfWork
/// </summary>
public interface IEntity<T>
{
    T Id { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    void SetUpdatedAt();
    void SetCreatedAt();
    void SetDeleted();
}
