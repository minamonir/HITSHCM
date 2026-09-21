using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Login;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages.Account;

public sealed class FirstLogonModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITenantCatalog _catalog;
    private readonly ITenantContext _tenant;

    public FirstLogonModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITenantCatalog catalog,
        ITenantContext tenant)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _catalog = catalog;
        _tenant = tenant;
    }

    public IActionResult OnGet()
    {
        if (!User.HasClaim(TenantClaimTypes.FirstLogonAck, "required"))
        {
            return LocalRedirect("~/");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        user.FirstLogonAckRequired = false;
        user.FirstLogonAckCompleted = true;
        await _userManager.UpdateAsync(user);

        var group = _catalog.Find(_tenant.BusinessGroupId) ?? _catalog.Find(user.BusinessGroupId);
        if (group is null)
        {
            throw new InvalidOperationException("Signed-in business group is no longer in the catalog.");
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        if (principal.Identity is not ClaimsIdentity identity)
        {
            throw new InvalidOperationException("Expected a claims identity after first-logon ack.");
        }

        LoginPrincipalBuilder.Apply(identity, group, user, mustChangePassword: false, firstLogonAckRequired: false);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { AllowRefresh = true, IsPersistent = false });

        return LocalRedirect("~/");
    }
}
