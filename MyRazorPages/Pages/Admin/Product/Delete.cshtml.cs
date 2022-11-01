using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Hubs;
using MyRazorPages.Models;
using MyRazorPages.Utils;

namespace MyRazorPages.Pages.Admin.Product
{
    [Authorize(UserRoles.Employee)]
    public class DeleteModel : PageModel
    {

        private readonly PRN221DBContext _context;
        private readonly IHubContext<ServerHub> hubContext;

        public DeleteModel(PRN221DBContext _context, IHubContext<ServerHub> hubContext)
        {
            this._context = _context;
            this.hubContext = hubContext;
        }
        public async Task<IActionResult> OnGet(int ProductId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == ProductId);
            if (_context.OrderDetails.Where(od => od.ProductId ==  product.ProductId).Count() != 0)
            {
                return Page();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadAdminProduct", await PaginatedList<Models.Product>.CreateAsync(_context.Products.Include(o => o.Category).OrderByDescending(o => o.ProductId).AsNoTracking(), 1, 10));
            return RedirectToPage("/Admin/Product/Index");
        }
    }
}
