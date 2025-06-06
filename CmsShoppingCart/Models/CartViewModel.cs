﻿using System.Collections.Generic;

namespace CmsShoppingCart.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public DireccionEnvio DireccionEnvio { get; set; }
        public List<DireccionEnvio> TodasLasDirecciones { get; set; }
    }
}
