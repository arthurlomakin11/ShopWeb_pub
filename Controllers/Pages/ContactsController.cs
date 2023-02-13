using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopWeb.Controllers
{
    public class ContactsControllerM : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Pages/ContactsM.cshtml");
        }
    }
    public class ContactsControllerF : Controller
    {
        public IActionResult Index()
        {
            return View("/Views/Pages/ContactsF.cshtml");
        }
    }
}
