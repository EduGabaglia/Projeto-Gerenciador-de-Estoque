using Microsoft.AspNetCore.Mvc;

namespace projeto_estoque_web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Produtos");
        }
    }
}