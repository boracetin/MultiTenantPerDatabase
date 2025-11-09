namespace MultitenantPerDb.Modules.Tenancy.Domain.Constants;

/// <summary>
/// Constants specific to Tenancy module
/// </summary>
public static class TenancyConstants
{
    public static class ConnectionStringKeys
    {
        public const string TenantConnection = "TenantConnection";
        public const string DesignTimeTenant1Connection = "DesignTimeTenant1Connection";
    }

    public static class TenantStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Suspended = "Suspended";
        public const string Deleted = "Deleted";
    }

    public static class ErrorMessages
    {
        public const string TenantNotFound = "Tenant not found";
        public const string TenantInactive = "Tenant is inactive";
        public const string InvalidTenantId = "Invalid tenant ID";
        public const string DuplicateTenantName = "Tenant name already exists";
    }

    public static class Validation
    {
        public const int TenantNameMaxLength = 100;
        public const int TenantNameMinLength = 3;
        public const int ConnectionStringMaxLength = 500;
    }
}
