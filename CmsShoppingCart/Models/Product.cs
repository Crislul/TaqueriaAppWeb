using CmsShoppingCart.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace CmsShoppingCart.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MinLength(2, ErrorMessage = "Minimo 2 caracteres!!!")]
        public string Name { get; set; }
        public string Slug { get; set; }

        [Required, MinLength(4, ErrorMessage = "Minimo 4 caracteres!!!")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Display(Name ="Category")]
        [Range(1, int.MaxValue, ErrorMessage ="Deberia elegir una categoria!!!")]
        public int CategoryId { get; set; }

       
        public string image { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }

        public int Stock { get; set; } // 📦 Agregado

    }
}
