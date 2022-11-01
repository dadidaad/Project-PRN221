using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Drawing.Printing;

namespace MyRazorPages.Pages.Category
{
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dBContext;

        public IndexModel(PRN221DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public Models.Category Category { get; set; }
        public PaginatedList<Models.Product> Products { get; set; }
        public List<Models.Category> Categories { get; set; }
        public async Task<IActionResult> OnGet(int? categoryId, int? pageIndex)
        {
            IQueryable<Models.Product> productIQ = dBContext.Products.Include(p => p.Category);
            if (categoryId != null || categoryId > 0)
            {
                Category = await dBContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
                productIQ = productIQ.Where(p => p.CategoryId == categoryId);
            }
            Products = await PaginatedList<Models.Product>.CreateAsync(productIQ.AsNoTracking(), pageIndex ?? 1, 4);
            Categories = await dBContext.Categories.ToListAsync();
            return Page();
        }
    }
}
