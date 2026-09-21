namespace Hitshcm.Web.Tenancy;

/// <summary>Claim types for server-resolved tenant (D-013a). Never a connection string.</summary>
public static class TenantClaimTypes
{
    public const string OrgId = "org_id";
    public const string BusinessGroupId = "bg_id";
}
