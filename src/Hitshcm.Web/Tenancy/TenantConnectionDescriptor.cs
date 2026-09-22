namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Server-side tenant connection handle after CURRENT <c>build_connectionString</c>.
/// <see cref="ConnectionString"/> is for SqlConnection on the server only — never claims (D-013a).
/// </summary>
public sealed record TenantConnectionDescriptor(
    string OrgId,
    string BusinessGroupId,
    string Provider,
    string OptionsName,
    string ApplicationName,
    string DataSource,
    string InitialCatalog,
    string ConnectionString)
{
    public override string ToString() =>
        $"{Provider}:{DataSource}/{InitialCatalog} ({ApplicationName})";
}
