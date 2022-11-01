using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Models;

namespace MyRazorPages.Pages.Product
{
    public class DetailModel : PageModel
    {
        private readonly PRN221DBContext dBContext;

        public DetailModel(PRN221DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public Models.Product Detail { get; set; }
        public IActionResult OnGet(int id)
        {
            Detail = dBContext.Products.Include(p => p.Category).FirstOrDefault(o => o.ProductId == id);
            return Page();
        }
    }
}
