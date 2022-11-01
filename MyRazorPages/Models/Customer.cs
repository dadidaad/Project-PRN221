using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRazorPages.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }
        public string CustomerId { get; set; } = null!;
        [Required(ErrorMessage ="Company name is required")]
        public string CompanyName { get; set; } = null!;
        [Required(ErrorMessage = "Contact name is required")]
        public string? ContactName { get; set; }
        [Required(ErrorMessage = "Contact title is required")]
        public string? ContactTitle { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public DateTime? CreatedDate { get; internal set; }
    }
}
