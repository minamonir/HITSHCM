namespace Hitshcm.Web.Login;

/// <summary>Stand-in for CURRENT <c>NasUsers</c> + <c>SysParam</c> rows read by <c>CheckURules</c>.</summary>
public sealed record LoginPolicyInput
{
    public required string LogonName { get; init; }

    public bool Inactive { get; init; }

    /// <summary>CURRENT <c>DateControl = 0000000003</c>.</summary>
    public bool MustChangePassword { get; init; }

    public bool PasswordNeverExpire { get; init; }

    public bool WindowsOnly { get; init; }

    public DateTimeOffset? PasswordDate { get; init; }

    public int PasswordExpireDays { get; init; } = 90;

    public DateTimeOffset UtcNow { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Employee pay/HR status is not active (CURRENT EmpAssignment / HRStatus).</summary>
    public bool EmployeeHrInactive { get; init; }

    public bool ForbiddenInactiveEmployees { get; init; }
}
