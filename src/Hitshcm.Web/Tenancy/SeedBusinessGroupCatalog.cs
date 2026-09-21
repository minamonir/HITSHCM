using Hitshcm.Web.Data;
using Hitshcm.Web.Identity;

namespace Hitshcm.Web.Tenancy;

/// <summary>In-process demo business groups and memberships for AUTH-1b. No connection strings.</summary>
public sealed class SeedBusinessGroupCatalog : ITenantCatalog, IBusinessGroupCatalog
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

    public IReadOnlyList<BusinessGroupInfo> GetMemberships(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = user.Email ?? user.UserName;
        if (string.Equals(key, IdentityDataSeeder.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.UserName, IdentityDataSeeder.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Groups;
        }

        if (string.Equals(key, IdentityDataSeeder.MultiBgEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Groups.Where(g =>
                    g.Id is IdentityDataSeeder.DefaultBusinessGroupId or HrId)
                .ToArray();
        }

        var own = Find(user.BusinessGroupId) ?? Find(IdentityDataSeeder.DefaultBusinessGroupId);
        return own is null ? [] : [own];
    }

    public TenantSecurityOptions GetSecurityOptions(string orgId)
    {
        _ = orgId;
        return new TenantSecurityOptions
        {
            PasswordExpireDays = 90,
            ForbiddenInactiveEmployees = true,
            EnableFirstLogonAck = false
        };
    }
}
