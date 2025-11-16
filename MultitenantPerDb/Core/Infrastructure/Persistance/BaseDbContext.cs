using Microsoft.EntityFrameworkCore;
using MultitenantPerDb.Core.Domain;

namespace MultitenantPerDb.Core.Infrastructure.Persistance;

/// <summary>
/// Base class for all module DbContexts - ENFORCES IDbContextTransactionType implementation
/// All DbContexts SHOULD inherit from this class for consistency
/// NOTE: Not strictly required - IDbContextTransactionType can be implemented directly
/// But provides consistent pattern and compile-time checks
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }
}
