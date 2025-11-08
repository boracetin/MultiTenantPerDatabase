using MediatR;
using MultitenantPerDb.Core.Application.Interfaces;
using MultitenantPerDb.Core.Domain.Exceptions;

namespace MultitenantPerDb.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior for rate limiting
/// Prevents API abuse and DDoS attacks by limiting request rates
/// Supports per-user, per-tenant, and global rate limits
/// </summary>
public class RateLimitingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRateLimitService _rateLimitService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RateLimitingBehavior<TRequest, TResponse>> _logger;

    public RateLimitingBehavior(
        IRateLimitService rateLimitService,
        ICurrentUserService currentUserService,
        ILogger<RateLimitingBehavior<TRequest, TResponse>> logger)
    {
        _rateLimitService = rateLimitService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Check if request requires rate limiting
        if (request is not IRateLimitedRequest rateLimitedRequest)
        {
            _logger.LogDebug("[RATE LIMIT SKIPPED] {RequestName} - Not an IRateLimitedRequest", requestName);
            return await next();
        }

        var userId = _currentUserService.UserId ?? "anonymous";
        var tenantId = _currentUserService.TenantId ?? "system";
        var limit = rateLimitedRequest.Limit;
        var windowSeconds = rateLimitedRequest.WindowSeconds;

        // Build rate limit key based on scope
        var rateLimitKey = rateLimitedRequest.Scope switch
        {
            RateLimitScope.Global => $"global:{requestName}",
            RateLimitScope.PerTenant => $"tenant:{tenantId}:{requestName}",
            RateLimitScope.PerUser => $"user:{userId}:{requestName}",
            RateLimitScope.PerUserPerTenant => $"user:{userId}:tenant:{tenantId}:{requestName}",
            _ => $"user:{userId}:tenant:{tenantId}:{requestName}"
        };

        _logger.LogDebug(
            "[RATE LIMIT CHECK] {RequestName} - Key: {Key} - Limit: {Limit}/{WindowSeconds}s - Scope: {Scope}",
            requestName,
            rateLimitKey,
            limit,
            windowSeconds,
            rateLimitedRequest.Scope);

        // Check rate limit
        var isAllowed = await _rateLimitService.IsAllowedAsync(rateLimitKey, limit, windowSeconds);

        if (!isAllowed)
        {
            var remaining = await _rateLimitService.GetRemainingRequestsAsync(rateLimitKey, limit, windowSeconds);
            var resetTime = await _rateLimitService.GetResetTimeAsync(rateLimitKey, windowSeconds);

            _logger.LogWarning(
                "[RATE LIMIT EXCEEDED] {RequestName} - Key: {Key} - Limit: {Limit}/{WindowSeconds}s - Remaining: {Remaining} - ResetIn: {ResetTime}s",
                requestName,
                rateLimitKey,
                limit,
                windowSeconds,
                remaining,
                resetTime);

            throw new RateLimitExceededException(limit, windowSeconds, resetTime);
        }

        var remainingRequests = await _rateLimitService.GetRemainingRequestsAsync(rateLimitKey, limit, windowSeconds);

        _logger.LogDebug(
            "[RATE LIMIT OK] {RequestName} - Key: {Key} - Remaining: {Remaining}/{Limit}",
            requestName,
            rateLimitKey,
            remainingRequests,
            limit);

        return await next();
    }
}
