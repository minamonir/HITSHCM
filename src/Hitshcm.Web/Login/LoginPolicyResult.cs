namespace Hitshcm.Web.Login;

public sealed record LoginPolicyResult(string Code, string Message)
{
    public bool IsOk => Code == LoginPolicyCodes.Ok;

    public bool IsMustChangePassword => Code == LoginPolicyCodes.MustChangePassword;

    public bool IsBlocked =>
        Code is LoginPolicyCodes.Inactive or LoginPolicyCodes.HrInactiveForbidden;

    public bool IsFailClosed =>
        Code is not (LoginPolicyCodes.Ok
            or LoginPolicyCodes.Inactive
            or LoginPolicyCodes.MustChangePassword
            or LoginPolicyCodes.HrInactiveForbidden);

    public static LoginPolicyResult Ok() => new(LoginPolicyCodes.Ok, string.Empty);

    public static LoginPolicyResult FailClosed() =>
        new(LoginPolicyCodes.FailClosed, LoginPolicyMessages.FailClosed);
}
