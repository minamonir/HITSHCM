namespace Hitshcm.Web.Tenancy;

/// <summary>Claim types for server-resolved tenant (D-013a). Never a connection string.</summary>
public static class TenantClaimTypes
{
    public const string OrgId = "org_id";
    public const string BusinessGroupId = "bg_id";
    public const string ProfileId = "profile_id";
    public const string EmpId = "emp_id";
    public const string MustChangePassword = "must_change_password";
    public const string FirstLogonAck = "first_logon_ack";
}
