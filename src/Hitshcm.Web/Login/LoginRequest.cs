namespace Hitshcm.Web.Login;

public sealed class LoginRequest
{
    public required string UserNameOrEmail { get; init; }

    public string? Password { get; init; }

    public string? BusinessGroupId { get; init; }

    public bool RememberMe { get; init; }

    public LoginAuthMethod Method { get; init; } = LoginAuthMethod.Password;

    /// <summary>
    /// SSO resume: already-authenticated external subject (email / NameId).
    /// Must match the local user. AUTH-1b does not challenge Entra/Okta.
    /// </summary>
    public string? ExternalSubject { get; init; }
}
