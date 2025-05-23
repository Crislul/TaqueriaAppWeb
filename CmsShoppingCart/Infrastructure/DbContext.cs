using Microsoft.EntityFrameworkCore;
using CmsShoppingCart.Models; // Cambia esto según la ubicación de tus modelos

namespace CmsShoppingCart.Infrastructure
{
    public class AppDbContext : DbContext
    {
        // Constructor que recibe las opciones del contexto
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define las tablas de la base de datos como DbSet
        public DbSet<Contacto> Contactos { get; set; } // Tabla Contactos
    }
}