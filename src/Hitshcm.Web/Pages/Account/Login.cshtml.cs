using System.ComponentModel.DataAnnotations;
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

    public LoginModel(ILoginOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyList<SelectListItem> BusinessGroups { get; private set; } = [];

    /// <summary>
    /// Live cloud logon shows Username + Password on first paint. The BG picker is a
    /// second step after password succeeds when the person has multiple memberships.
    /// </summary>
    public bool ShowBusinessGroupPicker { get; private set; }

    public string? IdpNotice { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

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

        return await HandleOutcomeAsync(outcome);
    }

    public IActionResult OnPostOffice365()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        ModelState.Clear();
        IdpNotice = LoginPolicyMessages.ExternalSsoNotReady;
        return Page();
    }

    private async Task<IActionResult> HandleOutcomeAsync(LoginOutcome outcome)
    {
        if (outcome.Status == LoginStatus.NeedsBusinessGroup && outcome.Memberships.Count > 0)
        {
            ShowBusinessGroupPicker = true;
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

    private void BindBusinessGroups(IReadOnlyList<BusinessGroupInfo> memberships)
    {
        var arabic = string.Equals(
            System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "ar",
            StringComparison.OrdinalIgnoreCase);

        BusinessGroups = memberships
            .Select(g => new SelectListItem(arabic ? g.NameAr : g.Name, g.Id, selected: false))
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
        [Display(Name = "Username")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Business group")]
        public string BusinessGroupId { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
