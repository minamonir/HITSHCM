using Hitshcm.Web.Identity;

namespace Hitshcm.Web.Tenancy;

/// <summary>In-process demo business groups for AUTH-1 login. No connection strings.</summary>
public sealed class SeedBusinessGroupCatalog : IBusinessGroupCatalog
{
    public const string HrId = "demo-bg-hr";
    public const string EastId = "demo-bg-east";
    public const string EastOrgId = "demo-org-east";

    private static readonly BusinessGroupInfo[] Groups =
    [
        new(
            IdentityDataSeeder.DefaultBusinessGroupId,
            IdentityDataSeeder.DefaultOrgId,
            "Demo HITS",
            "هيتس التجريبية"),
        new(
            HrId,
            IdentityDataSeeder.DefaultOrgId,
            "Demo HR",
            "الموارد البشرية التجريبية"),
        new(
            EastId,
            EastOrgId,
            "East Region",
            "المنطقة الشرقية")
    ];

    public IReadOnlyList<BusinessGroupInfo> List() => Groups;

    public BusinessGroupInfo? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return Groups.FirstOrDefault(g => string.Equals(g.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
