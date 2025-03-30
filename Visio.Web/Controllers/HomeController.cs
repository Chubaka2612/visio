using Microsoft.AspNetCore.Mvc;

namespace Visio.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
