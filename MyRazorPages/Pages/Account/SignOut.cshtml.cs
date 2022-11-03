using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorPages.Config;
using MyRazorPages.Utils;

namespace MyRazorPages.Pages.Account
{
    [Authorize(UserRoles.Customer, UserRoles.Employee)]
    public class SignOutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            return Redirect("~/Index");
        }
    }
}
