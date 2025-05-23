using System.ComponentModel.DataAnnotations;

namespace CmsShoppingCart.Models
{
    public class Page
    {
        public int Id { get; set; }
        [Required, MinLength(2, ErrorMessage ="Minimo 2 caracteres !!!")]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required, MinLength(4, ErrorMessage = "Minimo 4 caracteres !!!")]
        public string Content { get; set; }
        public int Sorting { get; set; }
    }
}
