using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorPages.Utils;

namespace MyRazorPages.Pages.Account
{
    [Authorize]
    public class SignOutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            return Redirect("~/Index");
        }
    }
}
