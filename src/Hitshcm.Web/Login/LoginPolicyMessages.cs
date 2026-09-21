namespace Hitshcm.Web.Login;

/// <summary>English fallbacks matching AUTH-1 login tests and CURRENT DC caption intent.</summary>
public static class LoginPolicyMessages
{
    public const string InvalidCredentials = "Invalid username/email or password.";
    public const string LockedOut = "This account is locked. Try again later.";
    public const string SelectBusinessGroup = "Select a valid business group.";
    public const string Inactive = "This account is inactive.";
    public const string HrInactiveForbidden = "Inactive employees are not allowed to sign in.";
    public const string FailClosed = "Sign-in could not be completed.";
    public const string ExternalSsoNotReady = "External sign-in is not configured on this host.";
}
