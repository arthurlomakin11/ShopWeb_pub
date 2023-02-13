using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Data;

namespace ShopWeb.Controllers
{
    public class ProductPageController : Controller
    {
        public IActionResult Index(int Id)
        {
            ShopWebContext Context = new();
            ViewBag.CurrentProduct = Context.Products.Include(prop => prop.Images).First(p => p.Id == Id);
            return View("./Views/Products/ProductPage.cshtml");
        }
    }
}
