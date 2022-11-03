using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyRazorPages.EmailService;
using MyRazorPages.Models;
using System.ComponentModel.DataAnnotations;

namespace MyRazorPages.Pages.Account
{
    public class ForgotModel : PageModel
    {
        private readonly PRN221DBContext dBContext;
        private readonly UserManager<Models.Account> _userManager;
        private readonly IEmailSender _emailSender;
        public ForgotModel(PRN221DBContext dBContext, UserManager<Models.Account> userManager, IEmailSender emailSender)
        {
            this.dBContext = dBContext;
            this._userManager = userManager; 
            this._emailSender = emailSender;
        }
        public void OnGet()
        {
        }
        [BindProperty]
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="Must correct format")]
        public string Email { get; set; }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                ViewData["msg_forgot"] = "Email doesn't exists";
                return Page();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                  "/Account/ResetPassword",
                  pageHandler: null,
                  values: new { token, email = user.Email },
                  protocol: Request.Scheme);
            var message = new Message(new string[] { user.Email },
                "Reset password token",
                $"Follow this <a href='{callbackUrl}'>link</a> to enter new password", null);
            await _emailSender.SendEmailAsync(message);
            return RedirectToPage("/Account/ForgotConfirmation");
        }
    }
}
