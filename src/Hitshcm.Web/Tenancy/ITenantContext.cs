namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Current tenant as resolved on the server from claims (D-013a).
/// AUTH-2 will map OrgId / BusinessGroupId to a live SQL connection via Key Vault.
/// AUTH-1b exposes `ITenantConnectionFactory` as a named-options / SQLite descriptor only.
/// </summary>
public interface ITenantContext
{
    string? OrgId { get; }

    string? BusinessGroupId { get; }
}
