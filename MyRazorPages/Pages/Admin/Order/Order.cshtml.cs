
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MyRazorPages.Pages.Admin.Order
{
    [Authorize(UserRoles.Employee)]
    public class OrderModel : PageModel
    {

        private readonly PRN221DBContext _context;
        private readonly IConfiguration configuration;
        public PaginatedList<Models.Order> Orders { get; set; }

        public DateTime? CurrentFilterFromDate { get; set; }
        public DateTime? CurrentFilterToDate { get; set; }
        public OrderModel(PRN221DBContext _context, IConfiguration configuration)
        {
            this._context = _context;
            this.configuration = configuration;
        }
        public IActionResult OnGetCancelOrder(int OrderId)
        {
            if (HttpContext.Session.Get("JWToken") != null )
            {
                var userEmail = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var acc = _context.Accounts.FirstOrDefault(a => a.Email.Equals(userEmail));
                if(acc.Role == 1)
                {
                    var order = _context.Orders.FirstOrDefault(o => o.OrderId == OrderId);
                    order.EmployeeId = acc.EmployeeId;
                    order.Employee = acc.Employee;
                    order.RequiredDate = null;
                    _context.Orders.Update(order);
                    _context.SaveChanges();
                }
            }
            return RedirectToPage("/Admin/Order/Order");
        }
        public IActionResult OnGetConfirmOrder(int OrderId)
        {
            if (HttpContext.Session.Get("JWToken") != null)
            {
                var userEmail = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var acc = _context.Accounts.FirstOrDefault(a => a.Email.Equals(userEmail));
                if (acc.Role == 1)
                {
                    var order = _context.Orders.FirstOrDefault(o => o.OrderId == OrderId);
                    order.EmployeeId = acc.EmployeeId;
                    order.Employee = acc.Employee;
                    order.ShippedDate = DateTime.Now;
                    _context.Orders.Update(order);
                    _context.SaveChanges();
                }
            }
            return RedirectToPage("/Admin/Order/Order");
        }
        public async Task<IActionResult> OnGet(int? pageIndex)
        {
            IQueryable<Models.Order> ordersIQ = _context.Orders.Include(o => o.Customer).Include(o => o.Employee);
            if (CurrentFilterFromDate.HasValue)
            {
                ordersIQ = ordersIQ.Where(s => s.OrderDate >= CurrentFilterFromDate);
            }
            if (CurrentFilterToDate.HasValue)
            {
                if(CurrentFilterFromDate.HasValue && CurrentFilterToDate.Value > CurrentFilterFromDate.Value)
                {
                    ViewData["date_noti"] = "To date must greater than from date";
                    ordersIQ = _context.Orders.Include(o => o.Customer).Include(o => o.Employee);
                }
                else
                {
                    ordersIQ = ordersIQ.Where(s => s.OrderDate <= CurrentFilterToDate);
                }
            }
            var pageSize = configuration.GetValue("PageSize", 10);
            Orders = await PaginatedList<Models.Order>.CreateAsync(ordersIQ.OrderByDescending(o => o.OrderDate).AsNoTracking(), pageIndex ?? 1, pageSize);
            return Page();
        }
        public static DateTime StringToDate(string datetime)
        {
            DateTime dt = DateTime.Parse(datetime);
            return dt;
        }
        public static string DateToString(DateTime dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dt.Year);
            sb.Append("-");
            if (dt.Month < 10)
                sb.Append("0");
            sb.Append(dt.Month);
            sb.Append("-");
            if (dt.Day < 10)
                sb.Append("0");
            sb.Append(dt.Day);
            return sb.ToString();
        }
    }
}
