using Microsoft.AspNetCore.Identity;

namespace Hitshcm.Web.Data;

/// <summary>
/// Local Identity user for the OpenIddict IdP (D-013).
/// Tenant keys are org/BG ids — never a SQL connection string (D-013a).
/// Policy fields are a CheckURules / NasUsers stand-in until DNACloudDB is wired.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Server-resolved organization id. Not a connection string.</summary>
    public string? OrgId { get; set; }

    /// <summary>Server-resolved business group id. Not a connection string.</summary>
    public string? BusinessGroupId { get; set; }

    /// <summary>CURRENT ProfileID placeholder.</summary>
    public string? ProfileId { get; set; }

    /// <summary>CURRENT EmpId placeholder.</summary>
    public string? EmpId { get; set; }

    /// <summary>CURRENT NasUsers.InActive → CheckURules 002.</summary>
    public bool Inactive { get; set; }

    /// <summary>CURRENT DateControl = 0000000003 → CheckURules 003.</summary>
    public bool MustChangePassword { get; set; }

    public DateTimeOffset? PasswordDate { get; set; }

    public bool PasswordNeverExpire { get; set; }

    /// <summary>CURRENT NasUsers.WindowsOnly — skips password-expiry 003.</summary>
    public bool WindowsOnly { get; set; }

    /// <summary>CURRENT HR/pay inactive employee → CheckURules 004 when org forbids it.</summary>
    public bool EmployeeHrInactive { get; set; }

    /// <summary>CURRENT NasUsers.ConsecutiveAttempts.</summary>
    public int ConsecutiveAttempts { get; set; }

    public DateTimeOffset? LastLogonDate { get; set; }

    public bool FirstLogonAckRequired { get; set; }

    public bool FirstLogonAckCompleted { get; set; }
}
