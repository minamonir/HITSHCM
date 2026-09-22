using Hitshcm.Web.Data;

namespace Hitshcm.Web.Tenancy;

/// <summary>
/// Business-group catalog + memberships for login (CURRENT setup DB <c>[businessgroup]</c> /
/// <c>BusinessGroupUsers</c> stand-in). Seeded demo data; SQL bind is <c>TenantSql</c> config.
/// </summary>
public interface ITenantCatalog : IBusinessGroupCatalog
{
    IReadOnlyList<BusinessGroupInfo> GetMemberships(ApplicationUser user);

    TenantSecurityOptions GetSecurityOptions(string orgId);
}
