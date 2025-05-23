namespace CmsShoppingCart.Models
{
    public class VentaDetalle
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Imagen { get; set; }  // Ruta o URL de la imagen
        public Venta Venta { get; set; }
    }
} 
