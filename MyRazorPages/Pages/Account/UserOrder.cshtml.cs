using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.EmailService;
using MyRazorPages.Hubs;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Security.Claims;
using System.Text.Json;

namespace MyRazorPages.Pages.Account
{
    [Authorize(UserRoles.Customer)]
    public class UserOrderModel : PageModel
    {
        private readonly IHubContext<ServerHub> hubContext;
        private readonly PRN221DBContext dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        public UserOrderModel(IHubContext<ServerHub> hubContext, PRN221DBContext dbContext, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            this.hubContext = hubContext;
            this.dbContext = dbContext;
            this._webHostEnvironment = webHostEnvironment;
            this._emailSender = emailSender;
        }

        public void OnGet()
        {
            var userId = Int32.Parse(HttpContext.User.Claims.First(c => c.Type == "USERID").Value);
            var user = dbContext.Accounts.Include(a => a.Employee).Include(a => a.Customer)
                .FirstOrDefault(a => a.AccountId == userId);
            ViewData["Role"] = user.Role == 1 ? "Employee" : "Customer";
            ViewData["Display name"] = user.Role == 1
                ? String.Concat(user.Employee.FirstName,
                ' ',
                user.Employee.LastName) : user.Customer.ContactName;
            List<Order> orders = dbContext.Orders.Include(a => a.OrderDetails).ThenInclude(b => b.Product).Where(o => o.CustomerId == user.CustomerId).OrderByDescending(x => x.OrderDate).ToList();
            ViewData["Order"] = orders.Count > 0 ? orders : null;
        }

        public async Task<IActionResult> OnGetExportInvoice(int OrderId)
        {
            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == OrderId);
            if(order == null)
            {
                return Page();
            }
            InvoiceHelper invoiceHelper = new InvoiceHelper(_webHostEnvironment, dbContext);
            IFormFile invoiceFilePdf = invoiceHelper.GenerateInvoice(OrderId);
            var userEmail = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var message = new Message(new string[] { userEmail },
               $"Export invoice for order {OrderId}" ,
               $"Your invoice here", new FormFileCollection { invoiceFilePdf});
            await _emailSender.SendEmailAsync(message);
            return RedirectToPage("/Account/UserOrder");
        }
        public async Task<IActionResult> OnGetCancelOrder(int OrderId)
        {
            using (var _context = new PRN221DBContext())
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == OrderId);
                order.RequiredDate = null;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("ReloadOrderAdmin", await PaginatedList<Order>.CreateAsync(_context.Orders.Include(o => o.Customer).Include(o => o.Employee).OrderByDescending(o => o.OrderDate).AsNoTracking(), 1, 10));
            }
            return RedirectToPage("/Account/UserOrder");
        }
    }
}
