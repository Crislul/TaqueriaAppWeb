using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PedidoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PedidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar los pedidos
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Pedidos()
        {
            // Obtener todos los pedidos, incluyendo los detalles de los productos, y ordenarlos por fecha descendente
            var pedidos = await _context.Ventas
                .Include(v => v.DireccionEnvio)  // Incluye la dirección de envío
                .Include(v => v.Detalles)  // Incluye los detalles (productos e imágenes)
                .OrderByDescending(v => v.FechaVenta)  // Ordena los pedidos por fecha descendente
                .ToListAsync();

            return View(pedidos);
        }

        // Acción para actualizar el estado de un pedido
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int id, EstadoVenta estado)
        {
            var venta = await _context.Ventas
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            // Convertir el enum a string antes de guardarlo en la base de datos
            venta.Estado = estado.ToString();  // Convertir el enum a string

            // Guardar los cambios en la base de datos
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Pedidos));
        }
    }
}