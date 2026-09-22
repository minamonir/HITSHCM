namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Per-BG SQL bind settings (CURRENT <c>[businessgroup]</c> server/database/security/user/password).
/// Passwords are plaintext from config / Key Vault — TripleDES decrypt stays at ingest, not here.
/// </summary>
public sealed class TenantSqlOptions
{
    public const string SectionName = "TenantSql";

    public string ApplicationNamePrefix { get; set; } = "HITSHCM";

    public int PacketSize { get; set; } = 8000;

    public bool PersistSecurityInfo { get; set; }

    public int ConnectionLifetime { get; set; }

    public Dictionary<string, TenantBusinessGroupSqlOptions> Groups { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TenantBusinessGroupSqlOptions
{
    /// <summary>CURRENT <c>server</c> → <c>data source=</c>.</summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>CURRENT <c>database</c> → <c>initial catalog=</c>.</summary>
    public string InitialCatalog { get; set; } = string.Empty;

    /// <summary>
    /// CURRENT <c>securityinfo</c>. <c>FALSE</c> → <c>integrated security=SSPI</c>;
    /// anything else → SQL auth with <see cref="UserId"/> / <see cref="Password"/>.
    /// </summary>
    public string SecurityInfo { get; set; } = "FALSE";

    public string? UserId { get; set; }

    /// <summary>Already-decrypted SQL password. Never write this into claims or HTML.</summary>
    public string? Password { get; set; }
}
