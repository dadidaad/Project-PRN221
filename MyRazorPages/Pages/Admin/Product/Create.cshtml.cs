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
    public class CreateModel : PageModel
    {
        private readonly PRN221DBContext _context;
        private readonly IHubContext<ServerHub> hubContext;
        public List<Models.Category> Categories { get; set; }

        public CreateModel(PRN221DBContext _context, IHubContext<ServerHub> hubContext)
        {
            this._context = _context;
            this.hubContext = hubContext;
        }
        [BindProperty]
        public Models.Product Product { get; set; }
        public IActionResult OnGet()
        {
            Categories =  _context.Categories.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            ModelState.Remove("Product.ProductId");
            if (!ModelState.IsValid)
            {
                return Page();
            }
            await _context.Products.AddAsync(Product);
            await _context.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("ReloadAdminProduct", await PaginatedList<Models.Product>.CreateAsync(_context.Products.Include(o => o.Category).OrderByDescending(o => o.ProductId).AsNoTracking(), 1, 10));
            return RedirectToPage("/Admin/Product/Index");
        }
    }
}
