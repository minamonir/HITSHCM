namespace Hitshcm.Web.Login;

/// <summary>
/// CheckURules-shaped result codes from CURRENT <c>logon.aspx.vb</c> /
/// <c>dbo.CheckURules</c>. TARGET maps these in <see cref="ILoginPolicyEvaluator"/>.
/// </summary>
public static class LoginPolicyCodes
{
    /// <summary>OK — continue logon.</summary>
    public const string Ok = "001";

    /// <summary><c>NasUsers.InActive = 1</c> — blocked.</summary>
    public const string Inactive = "002";

    /// <summary>
    /// Must change password: password aged past <c>PasswordExpireDays</c>
    /// or CURRENT <c>DateControl = 0000000003</c>.
    /// </summary>
    public const string MustChangePassword = "003";

    /// <summary>
    /// <c>SysParam.ForbiddenInactiveEmployees</c> and the employee is not HR-active — blocked.
    /// </summary>
    public const string HrInactiveForbidden = "004";

    /// <summary>TARGET fail-closed sentinel (CURRENT defaulted to 001 on proc error).</summary>
    public const string FailClosed = "000";
}
