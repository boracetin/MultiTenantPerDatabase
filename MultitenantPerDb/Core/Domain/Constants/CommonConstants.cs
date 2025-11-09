namespace MultitenantPerDb.Core.Domain.Constants;

/// <summary>
/// Common constants shared across all modules
/// </summary>
public static class CommonConstants
{
    public static class ResponseMessages
    {
        public const string Success = "Operation completed successfully";
        public const string NotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
        public const string BadRequest = "Invalid request";
        public const string InternalError = "An error occurred while processing your request";
    }

    public static class ValidationMessages
    {
        public const string Required = "{0} is required";
        public const string InvalidFormat = "{0} format is invalid";
        public const string MaxLength = "{0} cannot exceed {1} characters";
        public const string MinLength = "{0} must be at least {1} characters";
    }

    public static class DateTimeFormats
    {
        public const string DefaultDate = "yyyy-MM-dd";
        public const string DefaultDateTime = "yyyy-MM-dd HH:mm:ss";
        public const string DefaultTime = "HH:mm:ss";
    }

    public static class CacheKeys
    {
        public const string TenantPrefix = "tenant:";
        public const string UserPrefix = "user:";
    }
}
