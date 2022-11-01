using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazorPages.Models;
using System.Data.Entity;
using System.Text.Json;

namespace MyRazorPages.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dBContext;

        public IndexModel(PRN221DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public List<Models.Product> HotProducts { get; set; }
        public List<Models.Product> BestSaleProducts { get; set; }
        public List<Models.Product> NewProducts { get; set; }
        public List<Models.Category> Categories { get; set; }
        public IActionResult OnGet()
        {
            HotProducts = dBContext.Products.Include(p => p.Category).OrderByDescending(p => p.ReorderLevel).Take(4).ToList();
            BestSaleProducts =dBContext.Products.OrderByDescending(p => p.UnitsOnOrder).Take(4).ToList();
            NewProducts = dBContext.Products.OrderByDescending(p => p.ProductId).Take(4).ToList();
            Categories = dBContext.Categories.ToList();
            return Page();
        }
    }
}
