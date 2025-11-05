namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Marker interface to restrict DbContext access
/// Only infrastructure components (Repository, UnitOfWork) can access DbContext
/// Prevents direct database operations bypassing Repository pattern
/// </summary>
public interface ICanAccessDbContext
{
    // Marker interface - no methods needed
    // This ensures all database operations go through Repository layer
}
