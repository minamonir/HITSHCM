using Hitshcm.Web.Tenancy;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages;

public sealed class IndexModel : PageModel
{
    public IndexModel(ITenantContext tenant)
    {
        Tenant = tenant;
    }

    public ITenantContext Tenant { get; }

    public string? UserDisplay { get; private set; }

    public void OnGet()
    {
        UserDisplay = User.Identity?.Name;
    }
}
