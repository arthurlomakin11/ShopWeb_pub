using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopWeb.Models;
using ShopWeb.Shared;

using ShopWeb.Data;

namespace ShopWeb.Areas.Identity.Pages.Account.Manage
{
    public class LegalEntityModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public LegalEntityModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Є юридичною особою")]
            public bool IsLegalEntity { get; set; }

            [Display(Name = "Назва організації")]
            public string OrganizationName { get; set; }

            [Display(Name = "ЄДРПОУ")]
            public string LegalEntityID { get; set; }
        }

        private void Load(User user)
        {
            Input = new InputModel
            {
                OrganizationName = user.OrganizationName,
                IsLegalEntity = user.IsLegalEntity,
                LegalEntityID = user.LegalEntityID
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (int.Parse(SettingsManager.GetValue("LegalEntityEnabled")) != 1)
            {
                return LocalRedirect("Error");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Помилка '{_userManager.GetUserId(User)}'.");
            }

            Load(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (int.Parse(SettingsManager.GetValue("LegalEntityEnabled")) != 1)
            {
                return LocalRedirect("Error");
            }


            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Помилка '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                Load(user);
                return Page();
            }

            if (user.OrganizationName != Input.OrganizationName)
            {
                user.OrganizationName = Input.OrganizationName;
            }

            if (user.IsLegalEntity != Input.IsLegalEntity)
            {
                user.IsLegalEntity = Input.IsLegalEntity;
            }

            if (user.LegalEntityID != Input.LegalEntityID)
            {
                user.LegalEntityID = Input.LegalEntityID;
            }

            await _signInManager.UserManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Помилка";
            return RedirectToPage();
        }
    }
}
