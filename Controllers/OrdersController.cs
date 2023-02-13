using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ShopWeb.Data;
using ShopWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ShopWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopWeb.Controllers
{

    namespace CustomIdentityApp.Controllers
    {
        [Authorize]
        public class OrdersController : Controller
        {
            SignInManager<User> _signInManager;
            public OrdersController(SignInManager<User> signInManager)
            {
                _signInManager = signInManager;
            }
            public IActionResult Index()
            {
                SignManager SignManager = new()
                {
                    SignInManager = _signInManager,
                    Context = new ShopWebContext()
                };
                return View(SignManager.User.Carts
                    .Where(cart => cart.Status != StatusEnum.InCart)
                    .OrderByDescending(cart => cart.CartStatus.CreationDateTime)
                    .ToList());
            }

            public IActionResult Details(int cartId)
            {
                ShopWebContext Context = new ShopWebContext();
                SignManager SignManager = new SignManager
                {
                    SignInManager = _signInManager,
                    Context = Context
                };
                User User = SignManager.User;
                Cart CurrentCart = Context.Carts
                    .Where(cart => cart.Id == cartId)
                    .Where(cart => cart.Buyer == User)
                    .Where(cart => cart.Status != StatusEnum.InCart)
                    .Include(cart => cart.CartItems)
                        .ThenInclude(cart => cart.Product)
                    .AsSingleQuery()
                    .FirstOrDefault();
                if (CurrentCart != null)
                {
                    return View(CurrentCart);
                }
                else
                {
                    return new ErrorController().HttpStatusCodeHandler(404);
                }
            }
            /*[HttpPost]
            public async Task<IActionResult> Details(string userId)
            {
                // получаем пользователя
                User user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // получем список ролей пользователя
                    var userRoles = await _userManager.GetRolesAsync(user);
                    // получаем все роли
                    var allRoles = _roleManager.Roles.ToList();
                    // получаем список ролей, которые были добавлены
                    var addedRoles = roles.Except(userRoles);
                    // получаем роли, которые были удалены
                    var removedRoles = userRoles.Except(roles);

                    await _userManager.AddToRolesAsync(user, addedRoles);

                    await _userManager.RemoveFromRolesAsync(user, removedRoles);

                    return RedirectToAction("UserList");
                }

                return NotFound();
            }*/
        }
    }
}
