namespace Hitshcm.Web.Tenancy;

/// <summary>Login picker source for org/BG claims. AUTH-2 will replace the seed list with a SQL/Key Vault map.</summary>
public interface IBusinessGroupCatalog
{
    IReadOnlyList<BusinessGroupInfo> List();

    BusinessGroupInfo? Find(string? id);
}
