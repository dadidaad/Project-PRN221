using LinqToExcel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Hubs;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;

namespace MyRazorPages.Pages.Admin.Product
{
    [Authorize(UserRoles.Employee)]
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PRN221DBContext _context;
        private readonly IConfiguration configuration;
        private readonly IHubContext<ServerHub> hubContext;
        public PaginatedList<Models.Product> Products { get; set; }
        public List<Models.Category> Categories { get; set; }
        public String? SearchString { get; set; }
        public int? FilterOption { get; set; }
        public IndexModel(PRN221DBContext _context, IConfiguration configuration, IHubContext<ServerHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            this._context = _context;
            this.configuration = configuration;
            this.hubContext = hubContext;
            this._webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> OnGet(int? filterOption, String? searchString, int? pageIndex)
        {
            Categories = await _context.Categories.ToListAsync();
            IQueryable<Models.Product> productIQ = _context.Products.Include(o => o.Category);
            if (searchString != null && searchString.Trim().Length != 0)
            {
                SearchString = searchString;
            }
            if (filterOption != null)
            {
                FilterOption = filterOption;

            }
            if (SearchString != null)
            {
                productIQ = productIQ.Where(p => p.ProductName.Contains(SearchString));
            }
            if (FilterOption != null)
            {
                productIQ = productIQ.Where(p => p.CategoryId == FilterOption);
            }
            var pageSize = configuration.GetValue("PageSize", 10);
            Products = await PaginatedList<Models.Product>.CreateAsync(productIQ.OrderByDescending(o => o.ProductId).AsNoTracking(), pageIndex ?? 1, pageSize);
            return Page();

        }

        public async Task<IActionResult> OnPost(IFormFile FileUpload)
        {
            if (FileUpload != null)
            {
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string contentRootPath = _webHostEnvironment.ContentRootPath;
                    string targetpath = "";
                    targetpath = Path.Combine(contentRootPath, "wwwroot", "Docs");
                    string filename = FileUpload.FileName;
                    var savePath = Path.Combine(targetpath, filename);
                    bool exists = System.IO.Directory.Exists(targetpath);
                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(targetpath);
                    }
                    using (Stream fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        await FileUpload.CopyToAsync(fileStream);
                    }
                    string pathToExcelFile = savePath;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = getConfiguration().GetConnectionString("Excel03ConString");
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = getConfiguration().GetConnectionString("Excel07ConString");
                    }
                    connectionString = String.Format(connectionString, pathToExcelFile);
                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();
                    adapter.Fill(ds, "ExcelTable");
                    DataTable dtable = ds.Tables["ExcelTable"];
                    string sheetName = "Sheet1";
                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var productList = excelFile.Worksheet<Models.Product>(sheetName).Where(p => !String.IsNullOrEmpty(p.ProductName)).ToList();
                    foreach (var a in productList)
                    {
                        try
                        {
                            if (a.ProductName != "" && a.CategoryId != 0 && a.UnitsInStock != 0)
                            {
                                Models.Product product = new Models.Product();
                                product.ProductName = a.ProductName;
                                product.CategoryId = a.CategoryId;
                                product.QuantityPerUnit = a.QuantityPerUnit;
                                product.UnitPrice = a.UnitPrice;
                                product.UnitsInStock = a.UnitsInStock;
                                product.Discontinued = a.Discontinued;
                                await _context.Products.AddAsync(product);
                            }
                               
                        }
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {
                                    await Response.WriteAsync("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    await hubContext.Clients.All.SendAsync("ReloadAdminProduct", await PaginatedList<Models.Product>.CreateAsync(_context.Products.Include(o => o.Category).OrderByDescending(o => o.ProductId).AsNoTracking(), 1, 10));
                    //deleting excel file from folder
                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                }
            }
            return RedirectToPage("/Admin/Product/Index");
        }

        private IConfigurationRoot getConfiguration()
        {
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
    }
}
