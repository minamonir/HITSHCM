namespace Hitshcm.Web.Tenancy;

/// <summary>Login picker source for org/BG claims. Seed list until DNACloudDBBG is wired; SQL bind is <c>TenantSql</c> config.</summary>
public interface IBusinessGroupCatalog
{
    IReadOnlyList<BusinessGroupInfo> List();

    BusinessGroupInfo? Find(string? id);
}
