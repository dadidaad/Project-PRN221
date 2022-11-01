using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Data.Entity;

namespace MyRazorPages.Pages.Admin
{
    [Authorize(UserRoles.Employee)]
    public class DashboardModel : PageModel
    {
        private readonly PRN221DBContext _context;

        public DashboardModel(PRN221DBContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            decimal? weeklySale = _context.Orders.Where(e => e.OrderDate >= DateTime.Now.AddDays(-7) && e.ShippedDate != null).Sum(e => e.Freight);
            ViewData["weeklySale"] = weeklySale;

            decimal? totalOrder = _context.Orders.Where(e => e.ShippedDate != null).Sum(e => e.Freight);
            ViewData["totalOrder"] = totalOrder;

            decimal? totalCustomer = _context.Customers.Count();
            ViewData["totalCustomer"] = totalCustomer;

            decimal? totalGuest = totalCustomer - _context.Accounts.Count();
            ViewData["totalGuest"] = totalGuest;

            int currentMonth = DateTime.Now.Month;
            List<int> staticOrder = new List<int>();
            for (int i = 1; i <= currentMonth; i++)
            {
                int monthOrder = _context.Orders.Where(e => e.OrderDate.Value.Month == i && e.OrderDate.Value.Year == 2022).Count();
                staticOrder.Add(monthOrder);
            }
            ViewData["staticOrder"] = staticOrder;

            decimal newCustomer = _context.Customers.Where(e => e.CreatedDate >= DateTime.Now.AddDays(-30)).Count();

            ViewData["newCustomer"] = newCustomer;
        }
    }
}
