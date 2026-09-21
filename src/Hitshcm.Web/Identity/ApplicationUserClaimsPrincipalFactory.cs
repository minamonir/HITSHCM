using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hitshcm.Web.Identity;

public sealed class ApplicationUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.OrgId))
        {
            identity.AddClaim(new Claim(TenantClaimTypes.OrgId, user.OrgId));
        }

        if (!string.IsNullOrWhiteSpace(user.BusinessGroupId))
        {
            identity.AddClaim(new Claim(TenantClaimTypes.BusinessGroupId, user.BusinessGroupId));
        }

        // Defaults for OpenIddict / non-picker sign-in. Razor login replaces these
        // with the selected catalog group (D-013a) — never a connection string.
        return identity;
    }
}
