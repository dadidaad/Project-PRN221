using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.EmailService;
using MyRazorPages.Models;

namespace MyRazorPages.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly PRN221DBContext dBContext;
        private readonly UserManager<Models.Account> _userManager;
        private readonly IEmailSender _emailSender;
        public ResetPasswordModel(PRN221DBContext dBContext, UserManager<Models.Account> userManager, IEmailSender emailSender)
        {
            this.dBContext = dBContext;
            this._userManager = userManager;
            this._emailSender = emailSender;
        }
        [BindProperty]
        public ResetPassword ResetPassword { get; set; }
        public IActionResult OnGet(string token, string email)
        {
            ResetPassword = new ResetPassword { Token = token, Email = email };
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userManager.FindByEmailAsync(ResetPassword.Email);
            if(user == null)
            {
                ViewData["msg_reset"] = "Email doesn't exists";
                return Page();
            }
            var resetPassResult = await _userManager.ResetPasswordAsync(user, ResetPassword.Token, ResetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return Page();
            }
            return RedirectToPage("/Account/ResetPasswordConfirmation");
        }
    }
}
