using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyRazorPages.Pages.Account
{
    [Authorize(UserRoles.Employee, UserRoles.Customer)]
    public class UserProfileModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public UserProfileModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [BindProperty]
        public Customer Customer { get; set; }


        public void OnGet()
        {
            var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type == "USERID").Value);
            var user = dbContext.Accounts.Include(a => a.Employee).Include(a => a.Customer)
                .FirstOrDefault(a => a.AccountId == userId);
            ViewData["user"] = user;
            ViewData["Role"] = user.Role == 1 ? "Employee" : "Customer";
            ViewData["Display name"] = user.Role == 1
                ? String.Concat(user.Employee.FirstName,
                ' ',
                user.Employee.LastName) : user.Customer.ContactName;
        }

        public IActionResult OnPost()
        {
            ModelState.Remove("Customer.CustomerId");
            if (ModelState.IsValid)
            {
                var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type == "USERID").Value);
                var user = dbContext.Accounts.Include(a => a.Employee).Include(a => a.Customer)
                    .FirstOrDefault(a => a.AccountId == userId);
                user.Customer = Customer;
                Customer.CustomerId = user.CustomerId;
                dbContext.Accounts.Update(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetString("user", JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                }));
                return RedirectToAction("Account", "UserProfile");
            }
            return Page();
        }
    }
}
