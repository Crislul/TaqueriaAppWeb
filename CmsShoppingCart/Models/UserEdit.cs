using System.ComponentModel.DataAnnotations;

namespace CmsShoppingCart.Models
{
    public class UserEdit
    {
        [Required, EmailAddress]
        public string Correo { get; set; }

        [DataType(DataType.Password), MinLength(4, ErrorMessage = "Minimo 4 caracteres")]
        public string Contraseña { get; set; }

        public UserEdit() { }

        public UserEdit(AppUser appUser)
        {
            Correo = appUser.Email;
            Contraseña = appUser.PasswordHash;
        }
    }
}
