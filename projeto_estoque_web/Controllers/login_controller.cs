using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class LoginController : Controller
  {
      public IActionResult Index()
    {
      return View();
    }
    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
      if (model.Usuario == "Admin" && model.Senha == "@123")
      {
        return RedirectToAction("Index", "Dashboard");
      }
      ModelState.AddModelError("", "Usuário ou senha inválidos.");
      return View(model);
    }
  } 

  
}