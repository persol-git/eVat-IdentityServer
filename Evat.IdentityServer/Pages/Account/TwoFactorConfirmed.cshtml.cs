using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Evat.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class TwoFactorConfirmedModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
