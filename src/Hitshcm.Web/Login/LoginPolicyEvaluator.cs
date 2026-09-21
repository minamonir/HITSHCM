namespace Hitshcm.Web.Login;

/// <summary>
/// In-memory CheckURules stand-in. Codes match
/// <c>docs/inventory/db/DNACloudDB-schema/dbo/StoredProcedures/CheckURules.sql</c>.
/// </summary>
public sealed class LoginPolicyEvaluator : ILoginPolicyEvaluator
{
    public LoginPolicyResult Evaluate(LoginPolicyInput? input)
    {
        try
        {
            if (input is null || string.IsNullOrWhiteSpace(input.LogonName))
            {
                return LoginPolicyResult.FailClosed();
            }

            if (input.Inactive)
            {
                return new LoginPolicyResult(LoginPolicyCodes.Inactive, LoginPolicyMessages.Inactive);
            }

            if (input.ForbiddenInactiveEmployees && input.EmployeeHrInactive)
            {
                return new LoginPolicyResult(
                    LoginPolicyCodes.HrInactiveForbidden,
                    LoginPolicyMessages.HrInactiveForbidden);
            }

            if (!input.WindowsOnly && RequiresPasswordChange(input))
            {
                return new LoginPolicyResult(LoginPolicyCodes.MustChangePassword, string.Empty);
            }

            return LoginPolicyResult.Ok();
        }
        catch
        {
            return LoginPolicyResult.FailClosed();
        }
    }

    private static bool RequiresPasswordChange(LoginPolicyInput input)
    {
        if (input.MustChangePassword)
        {
            return true;
        }

        if (input.PasswordNeverExpire || input.PasswordExpireDays <= 0)
        {
            return false;
        }

        if (input.PasswordDate is null)
        {
            return false;
        }

        var expiresOn = input.PasswordDate.Value.AddDays(input.PasswordExpireDays);
        return expiresOn <= input.UtcNow;
    }
}
