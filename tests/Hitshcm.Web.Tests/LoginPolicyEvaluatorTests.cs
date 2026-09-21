using Hitshcm.Web.Login;

namespace Hitshcm.Web.Tests;

public sealed class LoginPolicyEvaluatorTests
{
    private readonly LoginPolicyEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_Ok_Returns001()
    {
        var result = _evaluator.Evaluate(BaseInput());

        Assert.Equal(LoginPolicyCodes.Ok, result.Code);
        Assert.True(result.IsOk);
        Assert.False(result.IsFailClosed);
    }

    [Fact]
    public void Evaluate_Inactive_Returns002()
    {
        var result = _evaluator.Evaluate(BaseInput() with { Inactive = true });

        Assert.Equal(LoginPolicyCodes.Inactive, result.Code);
        Assert.True(result.IsBlocked);
        Assert.Equal(LoginPolicyMessages.Inactive, result.Message);
    }

    [Fact]
    public void Evaluate_DateControlMustChange_Returns003()
    {
        var result = _evaluator.Evaluate(BaseInput() with { MustChangePassword = true });

        Assert.Equal(LoginPolicyCodes.MustChangePassword, result.Code);
        Assert.True(result.IsMustChangePassword);
        Assert.False(result.IsBlocked);
    }

    [Fact]
    public void Evaluate_ExpiredPassword_Returns003()
    {
        var result = _evaluator.Evaluate(BaseInput() with
        {
            PasswordNeverExpire = false,
            PasswordExpireDays = 30,
            PasswordDate = DateTimeOffset.UtcNow.AddDays(-45),
            UtcNow = DateTimeOffset.UtcNow
        });

        Assert.Equal(LoginPolicyCodes.MustChangePassword, result.Code);
    }

    [Fact]
    public void Evaluate_HrInactiveForbidden_Returns004()
    {
        var result = _evaluator.Evaluate(BaseInput() with
        {
            ForbiddenInactiveEmployees = true,
            EmployeeHrInactive = true
        });

        Assert.Equal(LoginPolicyCodes.HrInactiveForbidden, result.Code);
        Assert.True(result.IsBlocked);
        Assert.Equal(LoginPolicyMessages.HrInactiveForbidden, result.Message);
    }

    [Fact]
    public void Evaluate_InactiveWinsOverMustChange()
    {
        var result = _evaluator.Evaluate(BaseInput() with
        {
            Inactive = true,
            MustChangePassword = true
        });

        Assert.Equal(LoginPolicyCodes.Inactive, result.Code);
    }

    [Fact]
    public void Evaluate_WindowsOnly_SkipsPasswordExpiry003()
    {
        var result = _evaluator.Evaluate(BaseInput() with
        {
            WindowsOnly = true,
            MustChangePassword = true
        });

        Assert.Equal(LoginPolicyCodes.Ok, result.Code);
    }

    [Fact]
    public void Evaluate_NullInput_FailsClosed()
    {
        var result = _evaluator.Evaluate(null);

        Assert.Equal(LoginPolicyCodes.FailClosed, result.Code);
        Assert.True(result.IsFailClosed);
        Assert.Equal(LoginPolicyMessages.FailClosed, result.Message);
    }

    [Fact]
    public void Evaluate_BlankLogonName_FailsClosed()
    {
        var result = _evaluator.Evaluate(BaseInput() with { LogonName = "  " });

        Assert.True(result.IsFailClosed);
    }

    private static LoginPolicyInput BaseInput() => new()
    {
        LogonName = "admin@hitshcm.local",
        PasswordNeverExpire = true,
        ForbiddenInactiveEmployees = true
    };
}
