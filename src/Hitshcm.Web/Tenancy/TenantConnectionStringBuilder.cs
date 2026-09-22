namespace Hitshcm.Web.Tenancy;

/// <summary>
/// TARGET port of CURRENT <c>CommonLib.build_connectionString</c>.
/// Builds a classic ADO.NET SQL Server string. Does not open a connection.
/// </summary>
public static class TenantConnectionStringBuilder
{
    public static string Build(
        string dataSource,
        string initialCatalog,
        string securityInfo,
        string? userId,
        string? password,
        string applicationName,
        bool persistSecurityInfo = false,
        int packetSize = 8000,
        int connectionLifetime = 0)
    {
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new ArgumentException("Data source (server) is required.", nameof(dataSource));
        }

        if (string.IsNullOrWhiteSpace(initialCatalog))
        {
            throw new ArgumentException("Initial catalog (database) is required.", nameof(initialCatalog));
        }

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name is required for SQL audit.", nameof(applicationName));
        }

        var parts = new List<string>
        {
            "data source=" + dataSource.Trim(),
            "initial catalog=" + initialCatalog.Trim()
        };

        if (UsesIntegratedSecurity(securityInfo))
        {
            parts.Add("integrated security=SSPI");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required when securityinfo is not FALSE.", nameof(userId));
            }

            parts.Add("user id=" + userId.Trim());
            parts.Add("password=" + (password ?? string.Empty));
        }

        parts.Add("persist security info=" + (persistSecurityInfo ? "True" : "False"));
        parts.Add("Application Name=" + applicationName.Trim());
        parts.Add("Connection Lifetime=" + connectionLifetime);
        parts.Add("packet size=" + packetSize);

        return string.Join(";", parts) + ";";
    }

    public static bool UsesIntegratedSecurity(string? securityInfo) =>
        string.Equals(securityInfo?.Trim(), "FALSE", StringComparison.OrdinalIgnoreCase);
}
