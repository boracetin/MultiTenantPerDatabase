namespace MultitenantPerDb.Core.Application.Abstractions;

/// <summary>
/// Marker interface for queries that support caching
/// Queries implementing this interface will be cached by CachingBehavior
/// </summary>
public interface ICacheQuery
{
    /// <summary>
    /// Cache duration in minutes. Default is 5 minutes if not specified.
    /// </summary>
    int? CacheDurationMinutes => null;
}
