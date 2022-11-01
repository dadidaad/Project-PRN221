using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using MyRazorPages.Utils;

namespace MyRazorPages.Pages.Admin.Order
{
    [Authorize(UserRoles.Employee)]
    public class OrderDetailModel : PageModel
    {

        private readonly PRN221DBContext _context;

        public OrderDetailModel(PRN221DBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int OrderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == OrderId);
            var orderDetailList = _context.OrderDetails.Include(od => od.Product).Where(o => o.OrderId == OrderId).ToList();
            ViewData["order"] = order;
            ViewData["orderDetailList"] = orderDetailList;
            return Page();
        }
    }
}
