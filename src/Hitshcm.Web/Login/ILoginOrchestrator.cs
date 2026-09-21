namespace Hitshcm.Web.Login;

/// <summary>
/// Shared app login pipeline (CURRENT <c>dologon_cloud</c> / <c>dologonwithselectedbg</c>).
/// Password login and future SSO resume both call this — IdP callbacks must not bind tenant themselves.
/// </summary>
public interface ILoginOrchestrator
{
    Task<LoginOutcome> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// SSO resume hook: person already proven by an external IdP. AUTH-1b does not challenge Entra/Okta.
    /// </summary>
    Task<LoginOutcome> ResumeExternalAsync(
        string userNameOrEmail,
        string? businessGroupId,
        string externalSubject,
        bool rememberMe = false,
        CancellationToken cancellationToken = default);
}
