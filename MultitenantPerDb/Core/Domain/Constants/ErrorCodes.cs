namespace MultitenantPerDb.Core.Domain.Constants;

/// <summary>
/// Error codes used across the application
/// </summary>
public static class ErrorCodes
{
    // General errors (1000-1999)
    public const string GeneralError = "ERR_1000";
    public const string ValidationError = "ERR_1001";
    public const string NotFoundError = "ERR_1002";
    public const string UnauthorizedError = "ERR_1003";
    public const string ForbiddenError = "ERR_1004";

    // Database errors (2000-2999)
    public const string DatabaseError = "ERR_2000";
    public const string DuplicateRecordError = "ERR_2001";
    public const string ConcurrencyError = "ERR_2002";

    // Business logic errors (3000-3999)
    public const string BusinessRuleViolation = "ERR_3000";
    public const string InvalidOperation = "ERR_3001";

    // External service errors (4000-4999)
    public const string ExternalServiceError = "ERR_4000";
    public const string ExternalServiceTimeout = "ERR_4001";
}
