
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
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

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? CurrentFilterFromDate { get; set; }
        [BindProperty]
        [DataType(DataType.Date)]
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
        public async Task<IActionResult> OnGet(int? pageIndex, string currentFilterFromDate, string currentFilterToDate)
        {
            IQueryable<Models.Order> ordersIQ = _context.Orders.Include(o => o.Customer).Include(o => o.Employee);
            if (!string.IsNullOrEmpty(currentFilterFromDate))
            {
                CurrentFilterFromDate = DateTime.Parse(currentFilterFromDate);
                ordersIQ = ordersIQ.Where(s => s.OrderDate >= CurrentFilterFromDate);
            }
            if (!string.IsNullOrEmpty(currentFilterToDate))
            {
                CurrentFilterToDate = DateTime.Parse(currentFilterToDate);
                if(CurrentFilterFromDate.HasValue && CurrentFilterToDate.Value < CurrentFilterFromDate.Value)
                {
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
        public IActionResult OnGetExportExcel(string fromDate, string toDate)
        {
            // query data from database   

            var stream = new MemoryStream();
            //required using OfficeOpenXml;
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            IQueryable<Models.Order> ordersIQ = _context.Orders;
            string excelName = $"OrderList-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
            if(!string.IsNullOrEmpty(fromDate))
            {
                ordersIQ.Where(o => o.OrderDate.Value > DateTime.Parse(fromDate));
                excelName = excelName + $"-From:{fromDate}";
            }
            if(!string.IsNullOrEmpty(toDate))
            {
                ordersIQ.Where(o => o.OrderDate.Value < DateTime.Parse(toDate));
                excelName = excelName + $"-To:{toDate}";
            }
            var orderList = ordersIQ.Select(o => new
            {
                o.OrderId,
                OrderDate =  o.OrderDate.Value.ToString("dd-mm-yyyy"),
                RequiredDate = o.RequiredDate.Value.ToString("dd-mm-yyyy"),
                ShippedDate = o.ShippedDate.Value.ToString("dd-mm-yyyy"),
                o.CustomerId,
                o.EmployeeId,
                o.Freight,
                o.ShipName,
                o.ShipCity,
                o.ShipCountry,
                Status = GetStatus(o.OrderId)
            }).ToList();
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(orderList, true);
                package.Save();
            }
            stream.Position = 0;
            excelName = excelName + ".xlsx";
            return File(stream, "application/octet-stream", excelName);
            //return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
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

        public String GetStatus(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order.ShippedDate.HasValue)
            {
                return "Completed";
            }
            else if(order.RequiredDate.HasValue && !order.ShippedDate.HasValue)
            {
                return "Pending";
            }
            else if (!order.RequiredDate.HasValue)
            {
                return "Canceled";
            }
            return "";
        }
    }
}
