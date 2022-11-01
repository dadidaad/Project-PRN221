using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Models;
using System.Security.Principal;

namespace MyRazorPages.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public SignUpModel(PRN221DBContext dBContext)
        {
            this.dbContext = dBContext;
        }
        [BindProperty] 
        public Customer Customer { get; set; }
        [BindProperty]
        public Models.Account Account { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            ModelState.Remove("Customer.CustomerId");
            if (ModelState.IsValid)
            {
                var acc = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Email.Equals(Account.Email));
                if (acc != null)
                {
                    ViewData["msg"] = "Email is existed";
                    return Page();
                }
                else
                {
                    var cust = new Customer
                    {
                        CustomerId = GetCustID(),
                        CompanyName = Customer.CompanyName,
                        ContactName = Customer.ContactName,
                        ContactTitle = Customer.ContactTitle,
                        Address = Customer.Address,
                        CreatedDate = DateTime.Now,
                    };
                    var newAcc = new Models.Account()
                    {
                        Email = Account.Email,
                        Password = Account.Password,
                        CustomerId = cust.CustomerId,
                        Role = 2
                    };
                    await dbContext.Customers.AddAsync(cust);
                    await dbContext.Accounts.AddAsync(newAcc);
                    await dbContext.SaveChangesAsync();
                    return RedirectToPage("/index");
                }
            }
            else
            {
                return Page();
            }
        }

        private string GetCustID(int length = 5)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
