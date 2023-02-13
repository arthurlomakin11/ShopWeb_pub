using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopWeb.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ShopWeb.Controllers
{
    [Authorize(Roles = "ContentEditor")]
    public class ManagerController : Controller
    {
        readonly ShopWebContext Context = new();
        public IActionResult Index()
        {
            ViewBag.Context = Context;

            return View(Context.Menu
                .Where(item => item.Parent == null)
                .Where(item => item.Type == MenuType.MenuItem));
        }

        public IActionResult SliderIndex()
        {
            ViewBag.Context = Context;

            return View(Context.Menu
                .Where(item => item.Parent == null)
                .Where(item => item.Type == MenuType.SliderItem));
        }

        public IActionResult Edit(int MenuItemId)
        {
            MenuItem Item = Context.Menu.Find(MenuItemId);
            if (Item != null)
            {
                ViewData["Title"] = Item.Name;
                return View(Item);
            }

            return NotFound();
        }
        [HttpPost]
        public IActionResult Edit(int MenuItemId, string Content)
        {

            MenuItem Item = Context.Menu.Find(MenuItemId);
            if (Item != null)
            {
                Item.Content = Content;

                Context.Update(Item);
                Context.SaveChanges();

                return RedirectToAction("Index");
            }

            return NotFound();
        }        
    }
}
