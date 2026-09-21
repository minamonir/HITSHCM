namespace Hitshcm.Web.Tenancy;

/// <summary>CURRENT <c>SysParam</c> stand-in for CheckURules (per org). Not DNACloudDB.</summary>
public sealed class TenantSecurityOptions
{
    public int PasswordExpireDays { get; init; } = 90;

    public bool ForbiddenInactiveEmployees { get; init; }

    public bool EnableFirstLogonAck { get; init; }
}
