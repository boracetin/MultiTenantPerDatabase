namespace MultitenantPerDb.Shared.Kernel.Application.Abstractions;

/// <summary>
/// Marker interface to indicate that a command requires database transaction
/// Commands that don't implement this interface will skip transaction management
/// Use this for commands that only work with external APIs, cache, or in-memory operations
/// </summary>
public interface IApplicationDbTransactionalCommand
{
}
