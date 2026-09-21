using System.Security.Claims;
using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Authentication;

namespace Hitshcm.Web.Login;

public sealed class LoginOutcome
{
    public required LoginStatus Status { get; init; }

    public string? PolicyCode { get; init; }

    public string? ErrorMessage { get; init; }

    public ClaimsPrincipal? Principal { get; init; }

    public AuthenticationProperties? AuthProperties { get; init; }

    public string? RedirectPath { get; init; }

    public IReadOnlyList<BusinessGroupInfo> Memberships { get; init; } = [];

    public bool ShouldSignIn =>
        Principal is not null
        && Status is LoginStatus.Succeeded
            or LoginStatus.MustChangePassword
            or LoginStatus.NeedsFirstLogonAck;

    public static LoginOutcome InvalidCredentials() => new()
    {
        Status = LoginStatus.InvalidCredentials,
        ErrorMessage = LoginPolicyMessages.InvalidCredentials
    };

    public static LoginOutcome LockedOut() => new()
    {
        Status = LoginStatus.LockedOut,
        ErrorMessage = LoginPolicyMessages.LockedOut
    };

    public static LoginOutcome NeedsBusinessGroup(IReadOnlyList<BusinessGroupInfo> memberships) => new()
    {
        Status = LoginStatus.NeedsBusinessGroup,
        ErrorMessage = LoginPolicyMessages.SelectBusinessGroup,
        Memberships = memberships
    };

    public static LoginOutcome PolicyBlocked(string code, string message) => new()
    {
        Status = LoginStatus.PolicyBlocked,
        PolicyCode = code,
        ErrorMessage = message
    };

    public static LoginOutcome FailedClosed(string? message = null) => new()
    {
        Status = LoginStatus.FailedClosed,
        PolicyCode = LoginPolicyCodes.FailClosed,
        ErrorMessage = message ?? LoginPolicyMessages.FailClosed
    };
}
