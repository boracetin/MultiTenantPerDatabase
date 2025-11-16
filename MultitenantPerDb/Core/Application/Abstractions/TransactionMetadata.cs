using MultitenantPerDb.Core.Domain.Constants;

namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Metadata about a MediatR request's transaction requirements
/// Pre-scanned at startup - ZERO runtime reflection
/// </summary>
public class TransactionMetadata
{
    /// <summary>
    /// Request type (Command/Query)
    /// </summary>
    public Type RequestType { get; init; } = null!;
    
    /// <summary>
    /// Database types required by this request
    /// Pre-computed at startup from handler dependencies
    /// </summary>
    public HashSet<DatabaseType> RequiredDatabases { get; init; } = new();
    
    /// <summary>
    /// DbContext types used by the handler
    /// </summary>
    public HashSet<Type> RequiredDbContextTypes { get; init; } = new();
    
    /// <summary>
    /// Whether this request is read-only (no transaction needed)
    /// </summary>
    public bool IsReadOnly { get; init; }
}
