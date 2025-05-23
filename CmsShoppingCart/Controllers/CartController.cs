using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        private readonly CmsShoppingCartContext context;
        public CartController(CmsShoppingCartContext context)
        {
            this.context = context;
        }

        private List<CartItem> ObtenerCarrito()
        {
            return HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        //Get/Cart/
        public IActionResult Index()
        {
            List<CartItem> cart = ObtenerCarrito();

            var emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;
            DireccionEnvio direccion = null;

            if (!string.IsNullOrEmpty(emailUsuario))
            {
                var direccionSeleccionadaId = HttpContext.Session.GetInt32("DireccionSeleccionadaId");

                if (direccionSeleccionadaId.HasValue)
                {
                    direccion = context.DireccionesEnvio
                        .FirstOrDefault(d => d.Id == direccionSeleccionadaId.Value && d.Email == emailUsuario);
                }
                else
                {
                    direccion = context.DireccionesEnvio
                        .FirstOrDefault(d => d.Email == emailUsuario);
                }
            }

            CartViewModel cartVM = new CartViewModel
            {
                CartItems = cart,
                GrandTotal = cart.Sum(x => x.Price * x.Quantity),
                DireccionEnvio = direccion
            };

            return View(cartVM);
        }

        //Get/Cart/Add

        // Acción de agregar al carrito
        public async Task<IActionResult> Add(int id, string returnUrl = null)
        {
            Product product = await context.Products.FindAsync(id);
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            CartItem cartItem = cart.FirstOrDefault(x => x.ProductId == id);

            if (cartItem == null)
            {
                cart.Add(new CartItem(product));
            }
            else
            {
                cartItem.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return ViewComponent("SmallCart");
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        //Get/Cart/Decrease
        public IActionResult Decrease(int id)
            {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");

            CartItem cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if(cartItem.Quantity >1)
            {
                --cartItem.Quantity;
            }
            else{
                cart.RemoveAll(x =>x.ProductId == id);
            }

            HttpContext.Session.SetJson("Cart", cart);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("cart");
            }
            return RedirectToAction("Index");
            }

        //Get/Cart/Remove
        public IActionResult Remove(int id)
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");

            cart.RemoveAll(x => x.ProductId == id);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }else 
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }
        //Get/Cart/Limpiar
        public IActionResult Limpiar()
        {
            HttpContext.Session.Remove("Cart");
            //return RedirectToAction("Index");
            if (HttpContext.Request.Headers["X-Requested-With"] != "XMLHttpRequest")
                return Redirect(Request.Headers["Referer"].ToString());

            return Ok();
        }

        // Mostrar formulario de nueva dirección
        [HttpGet]
        public IActionResult DireccionEnvio()
        {
            return View();
        }

        // Guardar nueva dirección
        [HttpPost]
        public async Task<IActionResult> GuardarDireccionEnvio(DireccionEnvio nuevaDireccion)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Content("No se encontró el email del usuario autenticado.");

            nuevaDireccion.Email = email;

            ModelState.Remove(nameof(nuevaDireccion.Email)); // Se setea manualmente

            if (!ModelState.IsValid)
                return View("DireccionEnvio", nuevaDireccion);

            try
            {
                context.DireccionesEnvio.Add(nuevaDireccion);
                await context.SaveChangesAsync();

                // Guardar como seleccionada por default
                HttpContext.Session.SetInt32("DireccionSeleccionadaId", nuevaDireccion.Id);

                TempData["DireccionAgregada"] = "Dirección agregada exitosamente!";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                return Content($"Error al guardar la dirección: {ex.Message}");
            }
        }

        // Mostrar todas las direcciones del usuario
        public IActionResult MisDirecciones()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var direcciones = context.DireccionesEnvio
                .Where(d => d.Email == email)
                .ToList();

            var seleccionadaId = HttpContext.Session.GetInt32("DireccionSeleccionadaId");

            foreach (var d in direcciones)
                d.EsSeleccionada = (d.Id == seleccionadaId);

            return View(direcciones);
        }

        // Editar dirección
        public async Task<IActionResult> EditarDireccionEnvio(int id)
        {
            var direccion = await context.DireccionesEnvio.FindAsync(id);
            if (direccion == null)
                return NotFound();

            return View(direccion);
        }

        [HttpPost]
        public async Task<IActionResult> EditarDireccionEnvio(DireccionEnvio model)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            model.Email = email;
            ModelState.Remove(nameof(model.Email));

            if (!ModelState.IsValid)
                return View(model);

            var direccionExistente = await context.DireccionesEnvio
                .FirstOrDefaultAsync(d => d.Id == model.Id && d.Email == email);

            if (direccionExistente == null)
                return NotFound();

            direccionExistente.Calle = model.Calle;
            direccionExistente.Colonia = model.Colonia;
            direccionExistente.Ciudad = model.Ciudad;
            direccionExistente.CodigoPostal = model.CodigoPostal;
            direccionExistente.Telefono = model.Telefono;
            direccionExistente.Referencias = model.Referencias;

            await context.SaveChangesAsync();

            TempData["DireccionAgregada"] = "Dirección actualizada exitosamente!";
            return RedirectToAction("MisDirecciones");
        }

        // Eliminar dirección
        public async Task<IActionResult> EliminarDireccionEnvio(int id)
        {
            var direccion = await context.DireccionesEnvio.FindAsync(id);
            if (direccion == null)
                return NotFound();

            context.DireccionesEnvio.Remove(direccion);
            await context.SaveChangesAsync();

            TempData["DireccionAgregada"] = "Dirección eliminada exitosamente!";
            return RedirectToAction("MisDirecciones");
        }

        // Seleccionar una dirección
        [HttpPost]
        public async Task<IActionResult> SeleccionarDireccionEnvio(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var direcciones = await context.DireccionesEnvio
                .Where(d => d.Email == email)
                .ToListAsync();

            foreach (var d in direcciones)
                d.EsSeleccionada = false;

            var direccionSeleccionada = direcciones.FirstOrDefault(d => d.Id == id);
            if (direccionSeleccionada != null)
            {
                direccionSeleccionada.EsSeleccionada = true;
                HttpContext.Session.SetInt32("DireccionSeleccionadaId", id);
            }

            await context.SaveChangesAsync();
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(string metodoPago)
        {
            var emailUsuario = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailUsuario))
                return RedirectToAction("Login", "Account");

            var direccionSeleccionadaId = HttpContext.Session.GetInt32("DireccionSeleccionadaId");
            if (direccionSeleccionadaId == null)
            {
                TempData["Error"] = "Debes seleccionar una dirección de envío.";
                return RedirectToAction("MisDirecciones");
            }

            var carrito = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("Index");
            }

            var totalVenta = carrito.Sum(c => c.Price * c.Quantity);

            // Intenta convertir el método de pago recibido a enum
            if (!Enum.TryParse<Pago>(metodoPago, out var metodo))
            {
                metodo = Pago.TarjetaCredito; // Valor por defecto
            }

            var venta = new Venta
            {
                FechaVenta = DateTime.Now,
                Cliente = emailUsuario,
                MetodoPago = metodo.ToString(), // Asignar como string
                Estado = EstadoVenta.PendienteDePago.ToString(), // Asignar el enum directamente
                DireccionEnvioId = direccionSeleccionadaId.Value,
                Total = totalVenta,
                Imagen = carrito.FirstOrDefault()?.Image ?? "/media/products/", // Asigna la imagen de un producto o una predeterminada
                Detalles = new List<VentaDetalle>()
            };

            // Asignamos la imagen de uno de los productos (por ejemplo, el primero) para la venta
            var imagenVenta = carrito.FirstOrDefault()?.Image ?? "/media/products/";  // Usar la primera imagen del carrito o una predeterminada

            foreach (var item in carrito)
            {
                if (!string.IsNullOrEmpty(item.Image)) // Asegúrate de que la imagen no sea nula o vacía
                {
                    venta.Detalles.Add(new VentaDetalle
                    {
                        Producto = item.ProductName,
                        Cantidad = item.Quantity,
                        Precio = item.Price,
                        Imagen = item.Image // Verifica que la imagen no sea null o vacía
                    });
                }
                else
                {
                    // Si no hay imagen, puedes asignar una ruta predeterminada o dejarla vacía
                    venta.Detalles.Add(new VentaDetalle
                    {
                        Producto = item.ProductName,
                        Cantidad = item.Quantity,
                        Precio = item.Price,
                        Imagen = "/media/products/"
                    });
                }
            }

            // Lógica según el método de pago
            switch (metodo)
            {
                case Pago.PayPal:
                    // Lógica para PayPal (si se implementa)
                    break;
                case Pago.MercadoPago:
                    // Lógica para MercadoPago (si se implementa)
                    break;
                case Pago.PagoContraEntrega:
                    venta.Estado = EstadoVenta.Completada.ToString(); // Usar el enum directamente
                    break;
                case Pago.TarjetaCredito:
                case Pago.TarjetaDebito:
                    venta.Estado = EstadoVenta.Pagado.ToString(); // Usar el enum directamente
                    break;
            }

            context.Ventas.Add(venta);
            await context.SaveChangesAsync();

            venta.DireccionEnvio = await context.DireccionesEnvio
                .FirstOrDefaultAsync(d => d.Id == venta.DireccionEnvioId);

            HttpContext.Session.Remove("Cart");
            TempData["Mensaje"] = "¡Gracias por tu compra!";
            return View("Confirmacion", venta);
        }
    }
}
