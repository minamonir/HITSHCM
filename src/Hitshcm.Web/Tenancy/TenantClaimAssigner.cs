using System.Security.Claims;
using Hitshcm.Web.Data;

namespace Hitshcm.Web.Tenancy;

/// <summary>Puts org/BG ids on the identity. Never writes a connection string (D-013a).</summary>
public static class TenantClaimAssigner
{
    public static void Replace(ClaimsIdentity identity, BusinessGroupInfo group, ApplicationUser? user = null)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(group);

        GuardNotSecret(group.Id);
        GuardNotSecret(group.OrgId);

        RemoveAll(identity, TenantClaimTypes.OrgId);
        RemoveAll(identity, TenantClaimTypes.BusinessGroupId);
        RemoveAll(identity, TenantClaimTypes.ProfileId);
        RemoveAll(identity, TenantClaimTypes.EmpId);

        identity.AddClaim(new Claim(TenantClaimTypes.OrgId, group.OrgId));
        identity.AddClaim(new Claim(TenantClaimTypes.BusinessGroupId, group.Id));

        if (!string.IsNullOrWhiteSpace(user?.ProfileId))
        {
            GuardNotSecret(user.ProfileId);
            identity.AddClaim(new Claim(TenantClaimTypes.ProfileId, user.ProfileId));
        }

        if (!string.IsNullOrWhiteSpace(user?.EmpId))
        {
            GuardNotSecret(user.EmpId);
            identity.AddClaim(new Claim(TenantClaimTypes.EmpId, user.EmpId));
        }
    }

    public static void ReplaceFlag(ClaimsIdentity identity, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RemoveAll(identity, type);
        if (!string.IsNullOrWhiteSpace(value))
        {
            identity.AddClaim(new Claim(type, value));
        }
    }

    public static void GuardNotSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Server=", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Password=", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase)
            || value.Contains("user-server-database", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Refusing to put a connection secret or user-server-database composite into claims (D-013 / D-013a).");
        }
    }

    private static void RemoveAll(ClaimsIdentity identity, string type)
    {
        foreach (var claim in identity.FindAll(type).ToList())
        {
            identity.RemoveClaim(claim);
        }
    }
}
