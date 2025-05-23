using Microsoft.AspNetCore.Mvc;
using CmsShoppingCart.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CmsShoppingCart.Infrastructure;
using ClosedXML.Excel; // 📌 Para exportar a Excel
using System.IO;
using iTextSharp.text; // 📌 Para exportar a PDF
using iTextSharp.text.pdf;

namespace CmsShoppingCart.Controllers.Admin
{
    [Area("Admin")]
    public class ReporteVentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReporteVentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vista principal del reporte
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return View(ventas);
        }

        // Ver detalles de una venta
        public async Task<IActionResult> Detalle(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            return View(venta);
        }

        // 📌 Exportar a Excel
        public async Task<IActionResult> ExportarExcel()
        {
            var ventas = await _context.Ventas.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de Ventas");
                var currentRow = 1;

                // 📌 Encabezados
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Fecha";
                worksheet.Cell(currentRow, 3).Value = "Cliente";
                worksheet.Cell(currentRow, 4).Value = "Total";
                worksheet.Cell(currentRow, 5).Value = "Método de Pago";
                worksheet.Cell(currentRow, 6).Value = "Estado";

                // 📌 Datos
                foreach (var venta in ventas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = venta.Id;
                    worksheet.Cell(currentRow, 2).Value = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(currentRow, 3).Value = venta.Cliente;
                    worksheet.Cell(currentRow, 4).Value = venta.Total;
                    worksheet.Cell(currentRow, 5).Value = venta.MetodoPago;
                    worksheet.Cell(currentRow, 6).Value = venta.Estado;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteVentas.xlsx");
                }
            }
        }

        // 📌 Exportar a PDF
        public async Task<IActionResult> ExportarPDF()
        {
            var ventas = await _context.Ventas.ToListAsync();
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // 📌 Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Reporte de Ventas\n\n", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // 📌 Tabla
                PdfPTable table = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 10, 20, 25, 15, 15, 15 });

                // 📌 Encabezados
                string[] headers = { "ID", "Fecha", "Cliente", "Total", "Método Pago", "Estado" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)))
                    {
                        BackgroundColor = new BaseColor(200, 200, 200),
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // 📌 Datos
                foreach (var venta in ventas)
                {
                    table.AddCell(venta.Id.ToString());
                    table.AddCell(venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                    table.AddCell(venta.Cliente);
                    table.AddCell("$" + venta.Total.ToString("N2"));
                    table.AddCell(venta.MetodoPago);
                    table.AddCell(venta.Estado);
                }

                document.Add(table);
                document.Close();
                return File(ms.ToArray(), "application/pdf", "ReporteVentas.pdf");
            }
        }
    }
}