using System.ComponentModel.DataAnnotations;
using Hitshcm.Web.Identity;
using Hitshcm.Web.Login;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hitshcm.Web.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly ILoginOrchestrator _orchestrator;
    private readonly ITenantCatalog _tenants;

    public LoginModel(ILoginOrchestrator orchestrator, ITenantCatalog tenants)
    {
        _orchestrator = orchestrator;
        _tenants = tenants;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyList<SelectListItem> BusinessGroups { get; private set; } = [];

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        BindBusinessGroups();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        BindBusinessGroups();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var outcome = await _orchestrator.AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = Input.UserNameOrEmail,
            Password = Input.Password,
            BusinessGroupId = Input.BusinessGroupId,
            RememberMe = Input.RememberMe,
            Method = LoginAuthMethod.Password
        });

        if (outcome.Status == LoginStatus.NeedsBusinessGroup && outcome.Memberships.Count > 0)
        {
            BindBusinessGroups(outcome.Memberships);
        }

        if (outcome.ShouldSignIn && outcome.Principal is not null)
        {
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                outcome.Principal,
                outcome.AuthProperties ?? new AuthenticationProperties { AllowRefresh = true });

            if (!string.IsNullOrEmpty(outcome.RedirectPath) && Url.IsLocalUrl(outcome.RedirectPath))
            {
                return LocalRedirect(outcome.RedirectPath);
            }

            return LocalRedirect(GetSafeReturnUrl());
        }

        var error = string.IsNullOrWhiteSpace(outcome.ErrorMessage)
            ? LoginPolicyMessages.FailClosed
            : outcome.ErrorMessage;

        if (outcome.Status == LoginStatus.NeedsBusinessGroup)
        {
            ModelState.AddModelError("Input.BusinessGroupId", error);
        }
        else
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return Page();
    }

    private void BindBusinessGroups(IReadOnlyList<BusinessGroupInfo>? memberships = null)
    {
        var arabic = string.Equals(
            System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "ar",
            StringComparison.OrdinalIgnoreCase);

        var groups = memberships is { Count: > 0 } ? memberships : _tenants.List();

        if (string.IsNullOrWhiteSpace(Input.BusinessGroupId))
        {
            Input.BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId;
        }

        BusinessGroups = groups
            .Select(g => new SelectListItem(arabic ? g.NameAr : g.Name, g.Id))
            .ToList();
    }

    private string GetSafeReturnUrl()
    {
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return ReturnUrl;
        }

        return Url.Content("~/")!;
    }

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Username or email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Business group")]
        public string BusinessGroupId { get; set; } = IdentityDataSeeder.DefaultBusinessGroupId;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
