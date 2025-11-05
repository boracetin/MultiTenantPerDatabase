namespace MultitenantPerDb.Shared.Kernel.Domain;

/// <summary>
/// Marker interface to restrict UnitOfWork access
/// Only services implementing this interface can access IUnitOfWork
/// Prevents developers from accidentally accessing UnitOfWork in inappropriate places
/// </summary>
public interface ICanAccessUnitOfWork
{
    // Marker interface - no methods needed
    // This is a security boundary to control UnitOfWork access
}
