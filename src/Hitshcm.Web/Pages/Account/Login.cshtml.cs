using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Identity;
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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBusinessGroupCatalog _businessGroups;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IBusinessGroupCatalog businessGroups)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _businessGroups = businessGroups;
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

        var selectedGroup = _businessGroups.Find(Input.BusinessGroupId);
        if (selectedGroup is null)
        {
            ModelState.AddModelError("Input.BusinessGroupId", "Select a valid business group.");
        }

        if (!ModelState.IsValid || selectedGroup is null)
        {
            return Page();
        }

        var user = await FindUserAsync(Input.UserNameOrEmail);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            return Page();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            Input.Password,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await SignInWithBusinessGroupAsync(user, selectedGroup);
            return LocalRedirect(GetSafeReturnUrl());
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
        return Page();
    }

    private async Task SignInWithBusinessGroupAsync(ApplicationUser user, BusinessGroupInfo group)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        if (principal.Identity is not ClaimsIdentity identity)
        {
            throw new InvalidOperationException("Expected a claims identity after password sign-in.");
        }

        TenantClaimAssigner.Replace(identity, group);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                AllowRefresh = true
            });
    }

    private void BindBusinessGroups()
    {
        var arabic = string.Equals(
            System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "ar",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Input.BusinessGroupId))
        {
            Input.BusinessGroupId = IdentityDataSeeder.DefaultBusinessGroupId;
        }

        BusinessGroups = _businessGroups.List()
            .Select(g => new SelectListItem(arabic ? g.NameAr : g.Name, g.Id))
            .ToList();
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        return userNameOrEmail.Contains('@', StringComparison.Ordinal)
            ? await _userManager.FindByEmailAsync(userNameOrEmail) ?? await _userManager.FindByNameAsync(userNameOrEmail)
            : await _userManager.FindByNameAsync(userNameOrEmail) ?? await _userManager.FindByEmailAsync(userNameOrEmail);
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

        [Required]
        [Display(Name = "Business group")]
        public string BusinessGroupId { get; set; } = IdentityDataSeeder.DefaultBusinessGroupId;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
