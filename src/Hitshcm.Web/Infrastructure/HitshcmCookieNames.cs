namespace Hitshcm.Web.Infrastructure;

public static class HitshcmCookieNames
{
    /// <summary>HttpOnly BFF authentication cookie (Identity application scheme).</summary>
    public const string Auth = "Hitshcm.Auth";

    /// <summary>UI culture cookie (en / ar). Not used for tenant or SQL routing.</summary>
    public const string Culture = "Hitshcm.Culture";
}
