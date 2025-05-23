using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using CmsShoppingCart.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CmsShoppingCart.Controllers
{
    public class ContactosController : Controller
    {
        private readonly AppDbContext _context;

        // Inyección del contexto a través del constructor
        public ContactosController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enviar(string name, string email, string message, string telefono)
        {
            if (ModelState.IsValid)
            {
                // Crear el objeto del mensaje
                var contacto = new Contacto
                {
                    Nombre = name,
                    Correo = email,
                    Mensaje = message,
                    Fecha = DateTime.Now,
                    Telefono = telefono 
                };

                // Guardar en la base de datos
                _context.Contactos.Add(contacto);
                await _context.SaveChangesAsync();

                // Confirmar al usuario que el mensaje fue enviado
                TempData["SuccessMessage"] = "Tu mensaje ha sido enviado. Gracias por contactarnos.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Ocurrió un error al enviar tu mensaje. Inténtalo de nuevo.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Mensajes()
        {
            // Recuperar mensajes de la base de datos ordenados por fecha descendente
            var mensajes = await _context.Contactos
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View("~/Views/Contactos/Mensajes.cshtml", mensajes);
        }

        public IActionResult MensajesRecibidos()
        {
            var mensajes = _context.Contactos.ToList();  // Obtén todos los mensajes de la base de datos
            var mensajesNuevos = mensajes.Count(c => c.Fecha > DateTime.Now.AddHours(-24));  // Filtra y cuenta los mensajes nuevos

            ViewBag.MensajesNuevos = mensajesNuevos;  // Pasamos la cantidad de mensajes nuevos a la vista

            return View(mensajes);
        }
         
        public IActionResult Inicio()
        {
            var mensajesNuevos = _context.Contactos.Count(c => c.Fecha > DateTime.Now.AddHours(-24));
            ViewBag.MensajesNuevos = mensajesNuevos; // Pasamos el conteo de mensajes nuevos

            // Marcar los mensajes como leídos
            if (mensajesNuevos > 0)
            {
                var nuevosMensajes = _context.Contactos.Where(c => c.Fecha > DateTime.Now.AddHours(-24)).ToList();
                foreach (var mensaje in nuevosMensajes)
                {
                    mensaje.Leido = true; // Marcamos el mensaje como leído
                }

                _context.SaveChanges(); // Guarda los cambios
            }

            var contactos = _context.Contactos.ToList();
            return View(contactos);
        }
    }
}