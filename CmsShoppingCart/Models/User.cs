using System.ComponentModel.DataAnnotations;

namespace CmsShoppingCart.Models
{
    public class User
    {
        [Required, MinLength(2, ErrorMessage = "Minimo 2 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }

        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimo 4 caracteres")]
        public string Contraseña { get; set; }

    }
}
