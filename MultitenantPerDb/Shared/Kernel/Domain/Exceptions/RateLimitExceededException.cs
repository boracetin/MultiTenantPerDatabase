namespace MultitenantPerDb.Shared.Kernel.Domain.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class RateLimitExceededException : Exception
{
    public int Limit { get; }
    public int WindowSeconds { get; }
    public int RetryAfterSeconds { get; }

    public RateLimitExceededException(
        string message,
        int limit,
        int windowSeconds,
        int retryAfterSeconds) : base(message)
    {
        Limit = limit;
        WindowSeconds = windowSeconds;
        RetryAfterSeconds = retryAfterSeconds;
    }

    public RateLimitExceededException(
        int limit,
        int windowSeconds,
        int retryAfterSeconds)
        : this(
            $"Rate limit exceeded. Limit: {limit} requests per {windowSeconds} seconds. Retry after {retryAfterSeconds} seconds.",
            limit,
            windowSeconds,
            retryAfterSeconds)
    {
    }
}
