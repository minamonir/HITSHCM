using Hitshcm.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Hitshcm.Web.Pages.Culture;

[AllowAnonymous]
public sealed class SetModel : PageModel
{
    private readonly IOptions<RequestLocalizationOptions> _localization;

    public SetModel(IOptions<RequestLocalizationOptions> localization)
    {
        _localization = localization;
    }

    public IActionResult OnGet(string culture, string? returnUrl = "/")
    {
        var cultures = _localization.Value.SupportedUICultures ?? [];
        if (!cultures.Any(c => string.Equals(c.Name, culture, StringComparison.OrdinalIgnoreCase)))
        {
            culture = _localization.Value.DefaultRequestCulture.UICulture.Name;
        }

        Response.Cookies.Append(
            HitshcmCookieNames.Culture,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("~/");
    }
}
