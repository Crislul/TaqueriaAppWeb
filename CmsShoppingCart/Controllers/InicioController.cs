using Microsoft.AspNetCore.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class InicioController : Controller
    {
        // Acción para mostrar la vista Welcome.cshtml
        public IActionResult Welcome()
        {
            return View();  // Aquí buscará la vista Welcome.cshtml en Views/Inicio/
        }
    }
}