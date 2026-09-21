namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Demo / catalog business group. Ids only — never a SQL connection string (D-013a).
/// </summary>
public sealed record BusinessGroupInfo(string Id, string OrgId, string Name, string NameAr);
