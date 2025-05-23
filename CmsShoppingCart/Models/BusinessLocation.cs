using System;
using System.ComponentModel.DataAnnotations;

namespace CmsShoppingCart.Models
{
    public class BusinessLocation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

