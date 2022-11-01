using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Hubs;
using MyRazorPages.Models;
using MyRazorPages.Utils;
using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace MyRazorPages.Pages.Account
{
    public class CartModel : PageModel
    {
        private readonly PRN221DBContext dBContext;
        private readonly IHubContext<ServerHub> hubContext;
        public CartModel(PRN221DBContext dBContext, IHubContext<ServerHub> hubContext)
        {
            this.dBContext = dBContext;
            this.hubContext = hubContext;
        }
        [BindProperty]
        public Customer Customer { get; set; }
        [BindProperty]
        public DateTime? RequiredDate { get; set; }
        public async Task OnGet()
        {
            List<Cart> cart;
            if (HttpContext.Session.GetString("cart") == null)
            {
                cart = new List<Cart>();
            }
            else
            {
                cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            }
            ViewData["cart"] = cart;
            float totalPrice = 0.0f;
            foreach (var items in cart)
            {
                totalPrice += (float)dBContext.Products.
                    FirstOrDefault(p => p.ProductId == items.ProductId)
                    .UnitPrice * items.Quantity;
            }
            ViewData["total"] = totalPrice.ToString("0.00");
            if (HttpContext.Session.GetString("JWToken") != null)
            {
                var userId = HttpContext.User.Claims.First(c => c.Type == "USERID").Value;
                var user = await dBContext.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.AccountId == Int32.Parse("USERID"));
                this.Customer = user.Customer;
            }
        }

        public async Task<IActionResult> OnGetBuyNow(int productId)
        {
            var product = await dBContext.Products.Where(p => p.ProductId == productId).FirstOrDefaultAsync();
            if(product == null)
            {
                return NotFound();
            }
            List<Cart> cart;
            if (HttpContext.Session.GetString("cart") == null)
            {
                cart = new List<Cart>();
            }
            else
            {
                cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            }
            int index = Exists(cart, productId.ToString());
            if (index == -1){
                cart.Add(new Cart(product.ProductId, 1));
            }
            else
            {
                if (product.UnitsInStock > cart[index].Quantity)
                {
                    cart[index].Quantity++;
                }
                else
                {
                  ViewData["msgProduct"] = "Can not order product more than in stock";
                }
            }
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Account", "Cart");
        }

        public async Task<IActionResult> OnGetAddToCart(int productId)
        {
            var product = await dBContext.Products.Where(p => p.ProductId == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
            List<Cart> cart;
            if (HttpContext.Session.GetString("cart") == null)
            {
                cart = new List<Cart>();
            }
            else
            {
                cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            }
            int index = Exists(cart, productId.ToString());
            if (index == -1)
            {
                cart.Add(new Cart(product.ProductId, 1));
            }
            else
            {
                if (product.UnitsInStock > cart[index].Quantity)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    ViewData["msgProduct"] = "Can not order product more than in stock";
                    return RedirectToAction("Account", "Cart");
                }
            }
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
            await hubContext.Clients.All.SendAsync("ReloadCart", cart.Count);
            return Page();
        }
        public IActionResult OnGetDelete(string id)
        {
            var cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            int index = Exists(cart, id);
            cart.RemoveAt(index);
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Account", "Cart");
        }

        public IActionResult OnGetIncrease(string id)
        {
            var cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            int index = Exists(cart, id);
            var product = dBContext.Products.FirstOrDefault(p => p.ProductId == cart[index].ProductId);
            if (product.UnitsInStock > cart[index].Quantity)
            {
                cart[index].Quantity++;
            }
            else
            {
                ViewData["msgProduct"] = "Can not order product more than in stock";
            }
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Account", "Cart");
        }

        public IActionResult OnGetDecrease(string id)
        {
            var cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
            int index = Exists(cart, id);
            cart[index].Quantity--;
            if (cart[index].Quantity == 0)
            {
                cart.RemoveAt(index);
            }
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Account", "Cart");
        }
        public int Exists(List<Cart> carts, string id)
        {
            foreach (var item in carts)
            {
                if (item.ProductId == Int32.Parse(id))
                {
                    return carts.IndexOf(item);
                }
            }
            return -1;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.Get("cart") != null)
            {
                Boolean checkInsertCus = true;
                var cart = JsonSerializer.Deserialize<List<Cart>>(HttpContext.Session.GetString("cart"));
                if (HttpContext.Session.Get("JWToken") != null)
                {
                    var userId = HttpContext.User.Claims.First(c => c.Type == "USERID").Value;
                    var acc = await dBContext.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.AccountId == Int32.Parse("USERID"));
                    Customer.CustomerId = acc.CustomerId;
                    Customer = acc.Customer;
                    checkInsertCus = false;
                }
                else
                {
                    Customer.CustomerId = GetCustId();
                    Customer.CreatedDate = DateTime.Now;
                }
                Order order = new Order();
                order.CustomerId = Customer.CustomerId;
                order.OrderDate = DateTime.Now.Date;
                if (RequiredDate.Value.CompareTo(order.OrderDate) < 0)
                {
                    ViewData["msg"] = "Required date must larger than order date";
                    await OnGet();
                    return Page();
                }
                order.RequiredDate = RequiredDate;
                order.ShipName = Customer.ContactName;
                order.ShipAddress = Customer.Address;

                if (checkInsertCus)
                {
                    await dBContext.Customers.AddAsync(Customer);
                }
                await dBContext.Orders.AddAsync(order);
                await dBContext.SaveChangesAsync();
                double totalPrice = 0.0f;
                foreach (var x in cart)
                {
                    OrderDetail orderDetail = new OrderDetail();

                    orderDetail.OrderId = order.OrderId;
                    orderDetail.ProductId = x.ProductId;
                    var product = await dBContext.Products.FirstOrDefaultAsync(p => p.ProductId == x.ProductId);
                    orderDetail.UnitPrice = (decimal)product.UnitPrice;
                    orderDetail.Quantity = (short)x.Quantity;
                    product.UnitsInStock -= (short)x.Quantity;
                    product.UnitsOnOrder += (short)x.Quantity;
                    await dBContext.OrderDetails.AddAsync(orderDetail);
                    totalPrice += (float)orderDetail.UnitPrice * x.Quantity;
                }
                order.Freight = (decimal?)totalPrice;
                dBContext.Orders.Update(order);
                await dBContext.SaveChangesAsync();
                HttpContext.Session.Remove("cart");
                await hubContext.Clients.All.SendAsync("ReloadOrderAdmin", await PaginatedList<Order>.CreateAsync(dBContext.Orders.Include(o => o.Customer).Include(o => o.Employee).OrderByDescending(o => o.OrderDate).AsNoTracking(), 1, 10));
            }
              /* await PaginatedList<Order>.CreateAsync(dBContext.Orders.Include(o => o.Customer).Include(o => o.Employee).OrderByDescending(o => o.OrderDate).AsNoTracking(), 1, 10)*/
            //Include(o => o.Employee).Include(o => o.Customer).OrderByDescending(o => o.OrderDate).
            return RedirectToPage("/index");
        }

        private string GetCustId(int length = 5)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
