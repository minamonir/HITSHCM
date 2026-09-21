using System.Security.Claims;
using Hitshcm.Web.Data;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Hitshcm.Web.Login;

/// <summary>
/// TARGET port of CURRENT <c>dologon_cloud</c> → <c>dologonwithselectedbg</c>:
/// authenticate person → resolve BG → server connection factory → CheckURules → claims cookie.
/// </summary>
public sealed class LoginOrchestrator : ILoginOrchestrator
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITenantCatalog _catalog;
    private readonly ITenantConnectionFactory _connections;
    private readonly ILoginPolicyEvaluator _policy;
    private readonly ILogger<LoginOrchestrator> _logger;

    public LoginOrchestrator(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITenantCatalog catalog,
        ITenantConnectionFactory connections,
        ILoginPolicyEvaluator policy,
        ILogger<LoginOrchestrator> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _catalog = catalog;
        _connections = connections;
        _policy = policy;
        _logger = logger;
    }

    public Task<LoginOutcome> ResumeExternalAsync(
        string userNameOrEmail,
        string? businessGroupId,
        string externalSubject,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        return AuthenticateAsync(new LoginRequest
        {
            UserNameOrEmail = userNameOrEmail,
            BusinessGroupId = businessGroupId,
            RememberMe = rememberMe,
            Method = LoginAuthMethod.ExternalSso,
            ExternalSubject = externalSubject
        }, cancellationToken);
    }

    public async Task<LoginOutcome> AuthenticateAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindUserAsync(request.UserNameOrEmail);
        if (user is null)
        {
            return LoginOutcome.InvalidCredentials();
        }

        var person = await AuthenticatePersonAsync(user, request);
        if (person is not null)
        {
            return person;
        }

        var memberships = _catalog.GetMemberships(user);
        var selected = ResolveBusinessGroup(request.BusinessGroupId, memberships);
        if (selected is null)
        {
            return LoginOutcome.NeedsBusinessGroup(memberships);
        }

        try
        {
            _ = _connections.Resolve(selected.OrgId, selected.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant connection factory failed for org {OrgId} bg {BgId}.", selected.OrgId, selected.Id);
            return LoginOutcome.FailedClosed();
        }

        LoginPolicyResult policyResult;
        try
        {
            var security = _catalog.GetSecurityOptions(selected.OrgId);
            policyResult = _policy.Evaluate(new LoginPolicyInput
            {
                LogonName = user.UserName ?? user.Email ?? user.Id,
                Inactive = user.Inactive,
                MustChangePassword = user.MustChangePassword,
                PasswordNeverExpire = user.PasswordNeverExpire,
                WindowsOnly = user.WindowsOnly,
                PasswordDate = user.PasswordDate,
                PasswordExpireDays = security.PasswordExpireDays,
                EmployeeHrInactive = user.EmployeeHrInactive,
                ForbiddenInactiveEmployees = security.ForbiddenInactiveEmployees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login policy evaluator threw for {UserId}.", user.Id);
            return LoginOutcome.FailedClosed();
        }

        if (policyResult.IsFailClosed)
        {
            _logger.LogWarning("Login policy fail-closed for {UserId} code {Code}.", user.Id, policyResult.Code);
            return LoginOutcome.FailedClosed(policyResult.Message);
        }

        if (policyResult.IsBlocked)
        {
            await RecordAttemptAsync(user, success: false);
            return LoginOutcome.PolicyBlocked(policyResult.Code, policyResult.Message);
        }

        await RecordAttemptAsync(user, success: true);

        var firstLogonAck = user.FirstLogonAckRequired && !user.FirstLogonAckCompleted;
        var mustChange = policyResult.IsMustChangePassword;

        ClaimsPrincipal principal;
        try
        {
            principal = await _signInManager.CreateUserPrincipalAsync(user);
            if (principal.Identity is not ClaimsIdentity identity)
            {
                return LoginOutcome.FailedClosed();
            }

            LoginPrincipalBuilder.Apply(identity, selected, user, mustChange, firstLogonAck);
            GuardNoSecretsInIdentity(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build login principal for {UserId}.", user.Id);
            return LoginOutcome.FailedClosed();
        }

        var properties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            AllowRefresh = true
        };

        if (mustChange)
        {
            return new LoginOutcome
            {
                Status = LoginStatus.MustChangePassword,
                PolicyCode = LoginPolicyCodes.MustChangePassword,
                Principal = principal,
                AuthProperties = properties,
                RedirectPath = "/Account/ChangePassword",
                Memberships = memberships
            };
        }

        if (firstLogonAck)
        {
            return new LoginOutcome
            {
                Status = LoginStatus.NeedsFirstLogonAck,
                PolicyCode = policyResult.Code,
                Principal = principal,
                AuthProperties = properties,
                RedirectPath = "/Account/FirstLogon",
                Memberships = memberships
            };
        }

        return new LoginOutcome
        {
            Status = LoginStatus.Succeeded,
            PolicyCode = LoginPolicyCodes.Ok,
            Principal = principal,
            AuthProperties = properties,
            Memberships = memberships
        };
    }

    private async Task<LoginOutcome?> AuthenticatePersonAsync(ApplicationUser user, LoginRequest request)
    {
        switch (request.Method)
        {
            case LoginAuthMethod.ExternalSso:
                if (string.IsNullOrWhiteSpace(request.ExternalSubject)
                    || !SubjectMatchesUser(user, request.ExternalSubject))
                {
                    await RecordAttemptAsync(user, success: false);
                    return LoginOutcome.InvalidCredentials();
                }

                return null;

            case LoginAuthMethod.Password:
            default:
                if (string.IsNullOrEmpty(request.Password))
                {
                    await RecordAttemptAsync(user, success: false);
                    return LoginOutcome.InvalidCredentials();
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return null;
                }

                await RecordAttemptAsync(user, success: false);

                if (result.IsLockedOut)
                {
                    return LoginOutcome.LockedOut();
                }

                return LoginOutcome.InvalidCredentials();
        }
    }

    private BusinessGroupInfo? ResolveBusinessGroup(
        string? requestedId,
        IReadOnlyList<BusinessGroupInfo> memberships)
    {
        if (!string.IsNullOrWhiteSpace(requestedId))
        {
            var catalogGroup = _catalog.Find(requestedId);
            if (catalogGroup is null)
            {
                return null;
            }

            return memberships.FirstOrDefault(g =>
                string.Equals(g.Id, catalogGroup.Id, StringComparison.OrdinalIgnoreCase));
        }

        if (memberships.Count == 1)
        {
            return memberships[0];
        }

        return null;
    }

    private async Task RecordAttemptAsync(ApplicationUser user, bool success)
    {
        if (success)
        {
            user.ConsecutiveAttempts = 0;
            user.LastLogonDate = DateTimeOffset.UtcNow;
        }
        else
        {
            user.ConsecutiveAttempts++;
        }

        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            _logger.LogWarning(
                "Failed to persist login attempt counter for {UserId}: {Errors}",
                user.Id,
                string.Join("; ", update.Errors.Select(e => e.Description)));
        }
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            return null;
        }

        return userNameOrEmail.Contains('@', StringComparison.Ordinal)
            ? await _userManager.FindByEmailAsync(userNameOrEmail) ?? await _userManager.FindByNameAsync(userNameOrEmail)
            : await _userManager.FindByNameAsync(userNameOrEmail) ?? await _userManager.FindByEmailAsync(userNameOrEmail);
    }

    private static bool SubjectMatchesUser(ApplicationUser user, string subject)
    {
        return string.Equals(user.Email, subject, StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.UserName, subject, StringComparison.OrdinalIgnoreCase);
    }

    private static void GuardNoSecretsInIdentity(ClaimsIdentity identity)
    {
        foreach (var claim in identity.Claims)
        {
            TenantClaimAssigner.GuardNotSecret(claim.Value);
            TenantClaimAssigner.GuardNotSecret(claim.Type);
        }

        if (identity.Name is not null)
        {
            TenantClaimAssigner.GuardNotSecret(identity.Name);
        }
    }
}
