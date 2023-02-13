using Microsoft.AspNetCore.Mvc;

namespace ShopWeb.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error/{StatusCode}")]
        public IActionResult HttpStatusCodeHandler(int StatusCode)
        {
            ViewBag.StatusCode = StatusCode;
            return View("/Views/Pages/Error.cshtml");
        }
    }
}
