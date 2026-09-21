using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages.Account;

[AllowAnonymous]
public sealed class ForgotPasswordModel : PageModel
{
    public void OnGet()
    {
    }
}
