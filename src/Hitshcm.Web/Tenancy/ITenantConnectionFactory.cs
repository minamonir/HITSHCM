namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Resolves org/BG → connection descriptor from config/Key Vault later.
/// Development returns a SQLite named-options handle. Secrets never go on the cookie.
/// </summary>
public interface ITenantConnectionFactory
{
    TenantConnectionDescriptor Resolve(string orgId, string businessGroupId);
}
