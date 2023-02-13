using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ShopWeb.Data;
using System;
using System.Reflection;
using DynamicExpresso;
using ShopWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using ShopWeb.Shared;

namespace ShopWeb.Controllers
{
    public class MainController : Controller
    {
        readonly ShopWebContext Context = new();
        public dynamic Main(string MenuName, string SubMenuName)
        {
            MenuItem item = Context.Menu
                .Where(menu => menu.Parent == null)
                .Where(menu => menu.Name.Replace(" ", "") == MenuName)
                .Where(i => i.Active == true)
                .Where(i => i.Type == MenuType.MenuItem)
                .Include(i => i.Images)
                .FirstOrDefault();

            string RedirectUri = SettingsManager.GetValue("RedirectUri");

            if (string.IsNullOrWhiteSpace(MenuName))
            {
                if (!string.IsNullOrWhiteSpace(RedirectUri))
{
                    var encoded = Flurl.Url.EncodeIllegalCharacters(RedirectUri);
                    return Redirect(encoded);
                }
                StatisticsManager.MenuItemName = "Головна";
                return View("/Views/Pages/Main.cshtml");
            }
            else if (MenuName == "Головна")
            {
                StatisticsManager.MenuItemName = "Головна";
                return View("/Views/Pages/Main.cshtml");
            }
            else if (item != null)
            {
                StatisticsManager.MenuItem = item;
                if (!string.IsNullOrWhiteSpace(SubMenuName) && item.HasSubItems)
                {
                    MenuItem subitem = Context.Menu
                            .Where(menu => menu.Parent == item)
                            .Where(menu => menu.Name.Replace(" ", "") == SubMenuName)
                            .Where(item => item.Active == true)
                            .FirstOrDefault();
                    if (subitem != null)
                    {
                        if (string.IsNullOrWhiteSpace(subitem.Controller))
                        {
                            return View(subitem);
                        }
                        else
                        {
                            return GetController(subitem.Controller);
                        }
                    }
                    else
                    {
                        return new ErrorController().HttpStatusCodeHandler(404);
                    }
                }
                else if (string.IsNullOrWhiteSpace(item.Controller))
                {
                    return View(item);
                }
                else
                {
                    return GetController(item.Controller);
                }
            }
            else
            {
                return new ErrorController().HttpStatusCodeHandler(404);
            }            
        }
        public dynamic GetController(string ControllerName)
        {
            Type[] typelist = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "ShopWeb.Controllers");
            Type controller_type = typelist.FirstOrDefault(type => type.Name == ControllerName);
            if (controller_type != null)
            {
                ViewBag.Title = ControllerName;

                var interpreter = new Interpreter().Reference(controller_type);
                dynamic result = interpreter.Eval("new " + controller_type.Name + "().Index()");
                return result;
            }
            else
            {
                return new ErrorController().HttpStatusCodeHandler(404);
            }
        }
        Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
    }
}
