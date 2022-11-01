using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using Remotion.FunctionalProgramming;

namespace MyRazorPages.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly PRN221DBContext dBContext;

        public ProfileModel(PRN221DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public void OnGet()
        {
            var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type == "USERID").Value);
            var user = dBContext.Accounts.Include(a => a.Employee).Include(a => a.Customer)
                .FirstOrDefault(a => a.AccountId == userId);
            ViewData["Role"] = user.Role == 1 ? "Employee" : "Customer";
            ViewData["Display name"] = user.Role == 1
                ? String.Concat(user.Employee.FirstName,
                ' ',
                user.Employee.LastName) : user.Customer.ContactName; 
        }


    }
}
