using System.ComponentModel.DataAnnotations;
using Hitshcm.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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

        var user = await FindUserAsync(Input.UserNameOrEmail);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
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

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
