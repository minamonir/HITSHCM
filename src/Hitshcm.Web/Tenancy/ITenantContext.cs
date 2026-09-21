namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Current tenant as resolved on the server from claims (D-013a).
/// AUTH-2 will map OrgId / BusinessGroupId to a SQL connection via secure config — never from the identity name.
/// </summary>
public interface ITenantContext
{
    string? OrgId { get; }

    string? BusinessGroupId { get; }
}
