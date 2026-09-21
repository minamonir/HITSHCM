using System.Security.Claims;

namespace Hitshcm.Web.Tenancy;

/// <summary>Puts org/BG ids on the identity. Never writes a connection string (D-013a).</summary>
public static class TenantClaimAssigner
{
    public static void Replace(ClaimsIdentity identity, BusinessGroupInfo group)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(group);

        RemoveAll(identity, TenantClaimTypes.OrgId);
        RemoveAll(identity, TenantClaimTypes.BusinessGroupId);

        identity.AddClaim(new Claim(TenantClaimTypes.OrgId, group.OrgId));
        identity.AddClaim(new Claim(TenantClaimTypes.BusinessGroupId, group.Id));
    }

    private static void RemoveAll(ClaimsIdentity identity, string type)
    {
        foreach (var claim in identity.FindAll(type).ToList())
        {
            identity.RemoveClaim(claim);
        }
    }
}
