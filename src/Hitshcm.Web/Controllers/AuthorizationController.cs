using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Hitshcm.Web.Controllers;

/// <summary>
/// Local OpenIddict authorization server (D-013).
/// Entra (and other external IdPs) plug in later on this same gateway — not this slice.
/// Mode A session-exchange with NasDna Forms auth is out of scope (D-013b).
/// </summary>
public sealed class AuthorizationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form : Request.Query)
                },
                IdentityConstants.ApplicationScheme);
        }

        var user = await _userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var identity = await OpenIddictClaims.CreateIdentityAsync(_userManager, user);
        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(OpenIddictClaims.GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            var user = await FindUserAsync(request.Username);
            if (user is null)
            {
                return InvalidGrant("The username/password couple is invalid.");
            }

            var signIn = await _signInManager.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, lockoutOnFailure: true);
            if (!signIn.Succeeded)
            {
                return InvalidGrant("The username/password couple is invalid.");
            }

            var identity = await OpenIddictClaims.CreateIdentityAsync(_userManager, user);
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(OpenIddictClaims.GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var user = await _userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty);
            if (user is null)
            {
                return InvalidGrant("The token is no longer valid.");
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                return InvalidGrant("The user is no longer allowed to sign in.");
            }

            var identity = await OpenIddictClaims.CreateIdentityAsync(_userManager, user);
            identity.SetScopes(result.Principal!.GetScopes());
            identity.SetDestinations(OpenIddictClaims.GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _userManager.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
            claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
        }

        if (User.HasScope(Scopes.Profile))
        {
            claims[Claims.Name] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
            claims[Claims.PreferredUsername] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(user.OrgId))
        {
            claims[Tenancy.TenantClaimTypes.OrgId] = user.OrgId;
        }

        if (!string.IsNullOrWhiteSpace(user.BusinessGroupId))
        {
            claims[Tenancy.TenantClaimTypes.BusinessGroupId] = user.BusinessGroupId;
        }

        return Ok(claims);
    }

    private async Task<ApplicationUser?> FindUserAsync(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return username.Contains('@', StringComparison.Ordinal)
            ? await _userManager.FindByEmailAsync(username) ?? await _userManager.FindByNameAsync(username)
            : await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
    }

    private ForbidResult InvalidGrant(string description) =>
        Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }));
}
