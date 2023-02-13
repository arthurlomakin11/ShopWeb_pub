using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopWeb.Controllers
{
    [Authorize]
    public class Checkout : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Cart/Checkout.cshtml");
        }
    }
}
