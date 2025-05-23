using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace CmsShoppingCart.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, MinLength(2, ErrorMessage = "Minimo 2 caracteres!!!")]
        [RegularExpression(@"^[a-zA-Z-ñ ]+$", ErrorMessage ="Solo se permiten caracteres!!!")]
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sorting { get; set; }

    }
}
