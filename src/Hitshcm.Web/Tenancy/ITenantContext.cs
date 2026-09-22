namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Current tenant as resolved on the server from claims (D-013a).
/// SQL data source / catalog come from <see cref="ITenantConnectionFactory"/> — not from the cookie.
/// </summary>
public interface ITenantContext
{
    string? OrgId { get; }

    string? BusinessGroupId { get; }

    string? DataSource { get; }

    string? InitialCatalog { get; }
}
