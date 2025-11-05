namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Interface for all soft-deletable entities
/// </summary>
public abstract class SoftDeleteAudited
{
    public bool IsDeleted { get; protected set; } = false;
    public DateTime DeletedAt { get; protected set; }

    public void SetDeleted()
    {
        this.IsDeleted = true;
        this.DeletedAt = DateTime.UtcNow;
    }
}
