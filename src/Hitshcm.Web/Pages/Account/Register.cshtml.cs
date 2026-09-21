using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hitshcm.Web.Pages.Account;

[AllowAnonymous]
public sealed class RegisterModel : PageModel
{
    public void OnGet()
    {
    }
}
