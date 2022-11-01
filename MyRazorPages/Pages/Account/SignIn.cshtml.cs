using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using MyRazorPages.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MyRazorPages.Pages.Account
{
    public class SignInModel : PageModel
    {
        private readonly PRN221DBContext dBContext;
        private readonly UserManager<Models.Account> userManager;
        private readonly SignInManager<Models.Account> signInManager;
        public SignInModel(PRN221DBContext dBContext, UserManager<Models.Account> userManager, SignInManager<Models.Account> signInManager)
        {
            this.dBContext = dBContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var account = await userManager.FindByEmailAsync(Email);
                if (account == null)
                {
                    ViewData["msg"] = "Can not find account in db";
                    return Page();
                }
                else
                {
                    var result = await signInManager.CheckPasswordSignInAsync(account, Password, false);
                    if (result.Succeeded)
                    {
                        TokenProvider _tokenProvider = new TokenProvider(dBContext);
                        //Authenticate user
                        var userToken = _tokenProvider.LoginUser(account.AccountId.ToString(), account.Password);
                        if (userToken != null)
                        {
                            //Save token in session object
                            HttpContext.Session.SetString("JWToken", userToken);
                        }
                        if(account.Role == 1)
                        {
                            return RedirectToPage("/Admin/Dashboard");
                        }
                        return RedirectToPage("/index");
                    }
                    else
                    {
                        ViewData["msg"] = "Password isn't correct";
                        return Page();
                    }
                }
            }
            else
            {
                return Page();
            }
        }
    }
}
