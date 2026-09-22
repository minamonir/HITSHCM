using Microsoft.Extensions.Options;

namespace Hitshcm.Web.Tenancy;

/// <summary>
/// AUTH-2: org/BG → CURRENT-shaped SQL string from config (Key Vault later).
/// Does not open DNACloudDB. Secrets never go on the cookie.
/// </summary>
public sealed class TenantConnectionFactory : ITenantConnectionFactory
{
    private readonly ITenantCatalog _catalog;
    private readonly TenantSqlOptions _sql;

    public TenantConnectionFactory(ITenantCatalog catalog, IOptions<TenantSqlOptions> sql)
    {
        _catalog = catalog;
        _sql = sql.Value;
    }

    public TenantConnectionDescriptor Resolve(string orgId, string businessGroupId)
    {
        var group = _catalog.Find(businessGroupId)
            ?? throw new InvalidOperationException($"Unknown business group '{businessGroupId}'.");

        if (!string.Equals(group.OrgId, orgId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Organization does not match the selected business group.");
        }

        if (!_sql.Groups.TryGetValue(group.Id, out var bind) || bind is null)
        {
            throw new InvalidOperationException($"No TenantSql bind for business group '{group.Id}'.");
        }

        var applicationName = $"{_sql.ApplicationNamePrefix}-{group.OrgId}-{group.Id}";
        var connectionString = TenantConnectionStringBuilder.Build(
            bind.DataSource,
            bind.InitialCatalog,
            bind.SecurityInfo,
            bind.UserId,
            bind.Password,
            applicationName,
            _sql.PersistSecurityInfo,
            _sql.PacketSize,
            _sql.ConnectionLifetime);

        return new TenantConnectionDescriptor(
            OrgId: group.OrgId,
            BusinessGroupId: group.Id,
            Provider: "sqlserver",
            OptionsName: $"TenantSql:{group.Id}",
            ApplicationName: applicationName,
            DataSource: bind.DataSource.Trim(),
            InitialCatalog: bind.InitialCatalog.Trim(),
            ConnectionString: connectionString);
    }
}
