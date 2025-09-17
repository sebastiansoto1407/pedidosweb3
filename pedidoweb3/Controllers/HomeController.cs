using Microsoft.AspNetCore.Mvc;

namespace pedidoweb3.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
