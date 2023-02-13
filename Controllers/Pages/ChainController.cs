using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Data;
using System.Linq;

namespace ShopWeb.Controllers
{
    public class ChainController : Controller
    {
        ShopWebContext Context = new ShopWebContext();
        public IActionResult Index()
        {
            ViewBag.Shops = Context.Shops.ToList();
            return View("/Views/Pages/Chain.cshtml");
        }
    }
}
