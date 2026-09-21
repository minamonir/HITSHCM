using Hitshcm.Web.Tenancy;

namespace Hitshcm.Web.Infrastructure;

/// <summary>
/// CURRENT first-logon / DateControl continuation after cookie issue:
/// 003 → change password; first-logon ack → placeholder page.
/// </summary>
public sealed class LoginContinuationMiddleware
{
    private static readonly PathString[] AllowedWhenGated =
    [
        "/Account/Login",
        "/Account/Logout",
        "/Account/ChangePassword",
        "/Account/FirstLogon",
        "/Culture/Set",
        "/Error"
    ];

    private readonly RequestDelegate _next;

    public LoginContinuationMiddleware(RequestDelegate next) => _next = next;

    public Task Invoke(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return _next(context);
        }

        var path = context.Request.Path;
        if (IsExempt(path))
        {
            return _next(context);
        }

        if (user.HasClaim(TenantClaimTypes.MustChangePassword, "true"))
        {
            context.Response.Redirect("/Account/ChangePassword");
            return Task.CompletedTask;
        }

        if (user.HasClaim(TenantClaimTypes.FirstLogonAck, "required")
            && !path.StartsWithSegments("/Account/FirstLogon", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Account/FirstLogon");
            return Task.CompletedTask;
        }

        return _next(context);
    }

    private static bool IsExempt(PathString path)
    {
        if (path.StartsWithSegments("/connect", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/.well-known", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var allowed in AllowedWhenGated)
        {
            if (path.StartsWithSegments(allowed, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
