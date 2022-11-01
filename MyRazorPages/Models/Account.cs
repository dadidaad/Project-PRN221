using RazorPageEx2.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRazorPages.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string? Password { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage ="Must match to password")]
        public string? ConfirmPassword { get; set; }
        public string? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public int? Role { get; set; }
        public Boolean? IsActive { get; set;}
        
        public virtual Customer? Customer { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Role? RoleNavigation { get; set; }
    }
}
