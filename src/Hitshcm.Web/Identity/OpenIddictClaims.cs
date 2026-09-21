using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Hitshcm.Web.Identity;

internal static class OpenIddictClaims
{
    public static async Task<ClaimsIdentity> CreateIdentityAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
            .SetClaim(Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
            .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        if (!string.IsNullOrWhiteSpace(user.OrgId))
        {
            identity.SetClaim(TenantClaimTypes.OrgId, user.OrgId);
        }

        if (!string.IsNullOrWhiteSpace(user.BusinessGroupId))
        {
            identity.SetClaim(TenantClaimTypes.BusinessGroupId, user.BusinessGroupId);
        }

        return identity;
    }

    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject!.HasScope(Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case TenantClaimTypes.OrgId:
            case TenantClaimTypes.BusinessGroupId:
            case TenantClaimTypes.ProfileId:
            case TenantClaimTypes.EmpId:
                yield return Destinations.AccessToken;
                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
