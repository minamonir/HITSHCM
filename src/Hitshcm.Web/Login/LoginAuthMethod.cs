namespace Hitshcm.Web.Login;

public enum LoginAuthMethod
{
    /// <summary>CURRENT auth mode 3 — application password (Identity hasher).</summary>
    Password = 0,

    /// <summary>
    /// SSO resume after an IdP callback (CURRENT Entra/Okta return → <c>dologon_cloud</c>).
    /// Person is already proven; orchestrator still binds BG + policy.
    /// </summary>
    ExternalSso = 1
}
