using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain.Constants;

namespace MultitenantPerDb.Core.Domain;

/// <summary>
/// Marker interface for DbContext to declare its database type
/// Enables zero-reflection transaction grouping at runtime
/// </summary>
public interface ITransactionContext
{
    /// <summary>
    /// Database type for transaction grouping
    /// All contexts with same DatabaseType share single transaction
    /// </summary>
    static abstract DatabaseType DatabaseType { get; }
}
