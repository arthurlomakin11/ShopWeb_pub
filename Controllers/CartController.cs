using Microsoft.AspNetCore.Mvc;
using ShopWeb.Models;
using ShopWeb.Shared;

namespace ShopWeb.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            StatisticsManager.MenuItemName = "Cart";


            int CanUserBuyProducts = int.Parse(SettingsManager.GetValue("CanUserBuyProducts"));
            if (CanUserBuyProducts == 1)
            {
                return View("Cart");
            }
            else
            {
                return Redirect("/Error");
            }
        }
    }
}
