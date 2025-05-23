using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsShoppingCart.Models
{
    [Table("DireccionesEnvio")]
    public class DireccionEnvio
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; } // Aquí usarás el email
        [Required]
        public string Calle { get; set; }
        [Required]
        public string Colonia { get; set; }
        [Required]
        public string Ciudad { get; set; }
        [Required]
        public string CodigoPostal { get; set; }
        [Required]
        public string Telefono { get; set; }
        [Required]
        public string Referencias { get; set; }
        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool EsSeleccionada { get; set; } = false;
    }
}