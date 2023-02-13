using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ShopWeb.Data;

namespace ShopWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [EmailAddress(ErrorMessage = "Введіть Email у правильному форматі")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Пароль обов'язковий")]
            [StringLength(100, ErrorMessage = "Значення {0} має мати принаймні {2} та максимум {1} символів.", MinimumLength = 1)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Підтвердьте пароль")]
            [Compare("Password", ErrorMessage = "Пароль та підтвердження не збігаються.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "ПІБ обов'язкове")]
            [MaxLength(50, ErrorMessage = "Максимальна довжина ПІБ: 50 символів")]
            [Display(Name = "ПІБ")]
            public string Fio { get; set; }


            [Required(ErrorMessage = "Номер телефону обов'язковий")]
            [MaxLength(15, ErrorMessage = "Максимальна довжина паролю: 15 символів")]
            [Phone(ErrorMessage = "Введіть номер телефону у правильному форматі")]
            [Display(Name = "Номер телефону")]
            public string PhoneNumber { get; set; }
        }

        public string CustomPhoneNumber = LoginRegisterRoute.CustomPhoneNumber;

        /*[HttpPost]
        public JsonResult doesUserNameExist(string UserName)
        {

            var user = Membership.GetUser(UserName);

            return Json(user == null);
        }*/

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();


            /*ShopWebContext Context = new ShopWebContext();
            User DuplicateUser = Context.Users.FirstOrDefault(u => u.PhoneNumber == Input.PhoneNumber);

            if (DuplicateUser != null)
            {
                LoginModel.PhoneNumber = Input.PhoneNumber;
                return LocalRedirect("/Identity/Account/Login");
            }*/
            if (ModelState.IsValid)
            {
                var user = new User { UserName = Input.PhoneNumber.Replace("-", "").Replace("+38", ""), Email = Input.Email, Fio = Input.Fio, PhoneNumber = Input.PhoneNumber.Replace("-", "").Replace("+38", "") };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code, returnUrl},
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: true);
                        return LocalRedirect(returnUrl);
                    }
                }                
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
