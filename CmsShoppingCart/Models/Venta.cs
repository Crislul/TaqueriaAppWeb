using System;
using System.Collections.Generic;

namespace CmsShoppingCart.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; }
        public int DireccionEnvioId { get; set; }
        public string Imagen { get; set; }  // Ruta o URL de la imagen

        // Propiedad de navegación
        public DireccionEnvio DireccionEnvio { get; set; }
        public List<VentaDetalle> Detalles { get; set; }
    }

    public enum EstadoVenta
    {
        PendienteDePago,
        Pagado,
        Completada,
        Cancelada
    }
}
