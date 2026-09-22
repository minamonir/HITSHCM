namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Resolves org/BG → CURRENT <c>build_connectionString</c> descriptor from config / Key Vault.
/// Secrets never go on the cookie (D-013a).
/// </summary>
public interface ITenantConnectionFactory
{
    TenantConnectionDescriptor Resolve(string orgId, string businessGroupId);
}
