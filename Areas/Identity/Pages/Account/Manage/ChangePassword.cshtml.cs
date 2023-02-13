using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;
namespace ShopWeb.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Поточний пароль обов'язковий")]
            [DataType(DataType.Password)]
            [Display(Name = "Поточний пароль")]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "Новий пароль обов'язковий")]
            [StringLength(100, ErrorMessage = "Значення {0} має мати принаймні {2} та максимум {1} символів.", MinimumLength = 1)]
            [DataType(DataType.Password)]
            [Display(Name = "Новий пароль")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Підтвердьте новий пароль")]
            [Compare("NewPassword", ErrorMessage = "Новий пароль і пароль підтвердження не збігаються. Підтвердьте новий пароль")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            if (Base64Manager.Encode(Input.OldPassword) != SettingsManager.GetValue("GeneralAccountPassword"))
            {
                IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }
            else
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(user);

                await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
            }                       

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Користувач успішно змінив свій пароль.");
            StatusMessage = "Ваш пароль був змінений.";

            return RedirectToPage();
        }
    }
}
