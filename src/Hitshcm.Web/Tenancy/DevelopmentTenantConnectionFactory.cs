namespace Hitshcm.Web.Tenancy;

/// <summary>
/// AUTH-1b development factory: named Identity SQLite options, not DNACloudDB.
/// Does not return raw connection strings for claims (D-013a).
/// </summary>
public sealed class DevelopmentTenantConnectionFactory : ITenantConnectionFactory
{
    private readonly ITenantCatalog _catalog;

    public DevelopmentTenantConnectionFactory(ITenantCatalog catalog)
    {
        _catalog = catalog;
    }

    public TenantConnectionDescriptor Resolve(string orgId, string businessGroupId)
    {
        var group = _catalog.Find(businessGroupId)
            ?? throw new InvalidOperationException($"Unknown business group '{businessGroupId}'.");

        if (!string.Equals(group.OrgId, orgId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Organization does not match the selected business group.");
        }

        return new TenantConnectionDescriptor(
            OrgId: group.OrgId,
            BusinessGroupId: group.Id,
            Provider: "sqlite",
            OptionsName: "Identity",
            ApplicationName: $"HITSHCM-{group.OrgId}-{group.Id}");
    }
}
