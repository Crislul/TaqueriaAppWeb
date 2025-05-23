using System;
using System.ComponentModel.DataAnnotations;
 
namespace CmsShoppingCart.Models
{
    public class Contacto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; }

        public string Telefono { get; set; }

        public bool Leido { get; set; }

        public bool EsNuevo => Fecha > DateTime.Now.AddHours(-24); // Los mensajes de las últimas 24 horas son nuevos
    }
}
