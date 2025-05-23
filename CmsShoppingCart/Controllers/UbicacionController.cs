using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UbicacionController : Controller
{
    private readonly ApplicationDbContext _context;

    public UbicacionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Acción para mostrar la vista del mapa
    public async Task<IActionResult> Index()
    {
        // Obtener los datos de la ubicación del negocio desde la base de datos
        var location = await _context.BusinessLocations.FirstOrDefaultAsync();

        if (location == null)
        {
            // Si no se encuentra, mostrar una vista con un mensaje de error
            return View("Error", "No se encontró la ubicación del negocio en la base de datos.");
        }

        return View("~/Views/Ruta/Ubicacion.cshtml", location);
    }
}
