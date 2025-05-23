using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CmsShoppingCart.Models
{
    public class Login
    {
        [Required, EmailAddress]
        public string Correo { get; set; }

        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimo 4 caracteres")]
        public string Contraseña { get; set; }

        public string ReturnUrl { get; set; }

    }
}
