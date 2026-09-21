using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Tenancy;

namespace Hitshcm.Web.Login;

/// <summary>Attaches tenant + profile placeholders to the BFF identity. Never writes connection strings.</summary>
public static class LoginPrincipalBuilder
{
    public static void Apply(
        ClaimsIdentity identity,
        BusinessGroupInfo group,
        ApplicationUser user,
        bool mustChangePassword,
        bool firstLogonAckRequired)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(user);

        TenantClaimAssigner.Replace(identity, group, user);

        TenantClaimAssigner.ReplaceFlag(
            identity,
            TenantClaimTypes.MustChangePassword,
            mustChangePassword ? "true" : null);

        TenantClaimAssigner.ReplaceFlag(
            identity,
            TenantClaimTypes.FirstLogonAck,
            firstLogonAckRequired ? "required" : null);
    }
}
