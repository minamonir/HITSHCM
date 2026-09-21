namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Server-side tenant connection handle. Named options / provider only — never a secret in claims (D-013a).
/// CURRENT equivalent: <c>CommonLib.build_connectionString</c> after BG row lookup.
/// </summary>
public sealed record TenantConnectionDescriptor(
    string OrgId,
    string BusinessGroupId,
    string Provider,
    string OptionsName,
    string ApplicationName);
