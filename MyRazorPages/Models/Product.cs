using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRazorPages.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string ProductName { get; set; } = null!;
        [Required(ErrorMessage = "Required")]
        public int? CategoryId { get; set; }
        public string? QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        [Required(ErrorMessage = "Required")]
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
