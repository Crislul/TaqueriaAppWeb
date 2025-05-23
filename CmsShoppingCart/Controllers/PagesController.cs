using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class PagesController : Controller
{
    private readonly CmsShoppingCartContext context;

    public PagesController(CmsShoppingCartContext context)
    {
        this.context = context;
    }

    // GET: /welcome
    public IActionResult Welcome()
    {
        // Devuelve la vista estática de bienvenida
        return View("~/Views/Inicio/Welcome.cshtml");
    }

    // GET: /Horario
    public IActionResult Horario()
    {
        return View("~/Views/Pages/Horario.cshtml");
    }

    // GET: /{slug}
    public async Task<IActionResult> Page(string slug)
    {
        // Si no se proporciona un slug, se muestra la página "home" desde la base de datos
        if (string.IsNullOrEmpty(slug))
        {
            // Buscar la página "home" desde la base de datos
            var homePage = await context.pages
                .Where(x => x.Slug == "home")
                .FirstOrDefaultAsync();

            if (homePage != null)
            {
                return View(homePage); // Si encuentra la página "home", la devuelve
            }

            return NotFound(); // Si no se encuentra la página "home", retorna 404
        }

        // Busca la página según el slug proporcionado desde la base de datos
        Page page = await context.pages
            .Where(x => x.Slug == slug)
            .FirstOrDefaultAsync();

        if (page == null)
        {
            return NotFound(); // Si no se encuentra la página, retorna 404
        }

        return View(page); // Si encuentra la página, la devuelve
    }

    // Devuelve la vista de "SobreNosotros"
    [Route("pages/Sobre Nosotros")]
    public async Task<IActionResult> SobreNosotros()
    {
        var homePage = await context.pages
            .Where(x => x.Slug == "home")
            .FirstOrDefaultAsync();

        if (homePage != null)
        {
            return View(homePage);
        }

        return View("~/Views/Pages/SobreNosotros.cshtml");
    }


    // Devuelve la vista de "Servicios"
    public async Task<IActionResult> Servicios()
    {
        // Redirige a la página "home" si no existe una vista estática o en la base de datos
        var homePage = await context.pages
            .Where(x => x.Slug == "home")
            .FirstOrDefaultAsync();

        if (homePage != null)
        {
            return View(homePage);
        }

        return View("~/Views/Pages/Servicios.cshtml");
    }

    // Devuelve la vista de "Contactos"
    public async Task<IActionResult> Contactos()
    {
        // Redirige a la página "home" si no existe una vista estática o en la base de datos
        var homePage = await context.pages
            .Where(x => x.Slug == "home")
            .FirstOrDefaultAsync();

        if (homePage != null)
        {
            return View(homePage);
        }

        return View("~/Views/Pages/Contactos.cshtml");
    }
}
