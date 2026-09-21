using Microsoft.AspNetCore.Identity;

namespace Hitshcm.Web.Data;

/// <summary>
/// Local Identity user for the OpenIddict IdP (D-013).
/// Tenant keys are org/BG ids — never a SQL connection string (D-013a).
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Server-resolved organization id. Not a connection string.</summary>
    public string? OrgId { get; set; }

    /// <summary>Server-resolved business group id. Not a connection string.</summary>
    public string? BusinessGroupId { get; set; }
}
