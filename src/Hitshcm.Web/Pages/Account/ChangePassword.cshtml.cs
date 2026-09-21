using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Login;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages.Account;

public sealed class ChangePasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITenantCatalog _catalog;
    private readonly ITenantContext _tenant;

    public ChangePasswordModel(
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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Forced { get; private set; }

    public IActionResult OnGet()
    {
        Forced = User.HasClaim(TenantClaimTypes.MustChangePassword, "true");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Forced = User.HasClaim(TenantClaimTypes.MustChangePassword, "true");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        user.MustChangePassword = false;
        user.PasswordDate = DateTimeOffset.UtcNow;
        user.PasswordNeverExpire = false;
        await _userManager.UpdateAsync(user);

        await ReissueCookieAsync(user);
        return LocalRedirect(NextPath(user));
    }

    private async Task ReissueCookieAsync(ApplicationUser user)
    {
        var group = _catalog.Find(_tenant.BusinessGroupId) ?? _catalog.Find(user.BusinessGroupId);
        if (group is null)
        {
            throw new InvalidOperationException("Signed-in business group is no longer in the catalog.");
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        if (principal.Identity is not ClaimsIdentity identity)
        {
            throw new InvalidOperationException("Expected a claims identity after password change.");
        }

        var firstLogon = user.FirstLogonAckRequired && !user.FirstLogonAckCompleted;
        LoginPrincipalBuilder.Apply(identity, group, user, mustChangePassword: false, firstLogonAckRequired: firstLogon);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { AllowRefresh = true, IsPersistent = false });
    }

    private static string NextPath(ApplicationUser user) =>
        user.FirstLogonAckRequired && !user.FirstLogonAckCompleted
            ? "/Account/FirstLogon"
            : "/";

    public sealed class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
