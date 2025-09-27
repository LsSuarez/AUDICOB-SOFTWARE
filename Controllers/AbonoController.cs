using Audicob.Data;
using Audicob.Models;
using Audicob.Models.ViewModels.Cobranza;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Audicob.Controllers
{
    [Authorize(Roles = "Cliente")] // Control de acceso por rol
    public class AbonoController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AbonoController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Ver estado de cuenta
        public async Task<IActionResult> EstadoCuenta()
        {
            var userId = User.Identity.Name; // Obtiene el UserId del usuario autenticado

            // Asegúrate de que "UserId" esté correctamente asociado con el cliente
            var cliente = await _db.Clientes
                .Where(c => c.UserId == userId) // Asegúrate de que 'UserId' esté correctamente relacionado en el modelo Cliente
                .Include(c => c.Deuda)
                .FirstOrDefaultAsync();

            if (cliente == null || cliente.Deuda == null)
            {
                TempData["Error"] = "Cliente o deuda no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var estadoCuenta = new EstadoCuentaViewModel
            {
                TotalDeuda = cliente.Deuda.TotalAPagar, // Asegúrate de que 'TotalAPagar' esté correctamente calculado en el modelo Deuda
                Capital = cliente.Deuda.Monto,
                Intereses = cliente.Deuda.PenalidadCalculada,
                HistorialTransacciones = await _db.Transacciones
                    .Where(t => t.ClienteId == cliente.Id)
                    .OrderByDescending(t => t.Fecha)  // Ordenar por fecha descendente
                    .ToListAsync()
            };

            return View(estadoCuenta);
        }

        // Filtro por fechas y montos
        public async Task<IActionResult> FiltrarHistorial(string searchTerm, decimal? montoMin, decimal? montoMax)
        {
            var userId = User.Identity.Name; // Obtiene el UserId del usuario autenticado

            var cliente = await _db.Clientes
                .Where(c => c.UserId == userId)
                .Include(c => c.Deuda)
                .FirstOrDefaultAsync();

            if (cliente == null || cliente.Deuda == null)
            {
                TempData["Error"] = "Cliente o deuda no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var transacciones = _db.Transacciones
                .Where(t => t.ClienteId == cliente.Id)
                .AsQueryable();

            // Filtrar por descripción
            if (!string.IsNullOrEmpty(searchTerm))
                transacciones = transacciones.Where(t => t.Descripcion.Contains(searchTerm));

            // Filtrar por monto mínimo
            if (montoMin.HasValue)
                transacciones = transacciones.Where(t => t.Monto >= montoMin);

            // Filtrar por monto máximo
            if (montoMax.HasValue)
                transacciones = transacciones.Where(t => t.Monto <= montoMax);

            var historial = await transacciones
                .OrderByDescending(t => t.Fecha)  // Ordenar por fecha descendente
                .ToListAsync();

            return PartialView("_HistorialTransacciones", historial); // Devolver la vista parcial con los datos filtrados
        }

        // Ver detalle del comprobante de pago
        public async Task<IActionResult> VerComprobante(int transaccionId)
        {
            var transaccion = await _db.Transacciones
                .Where(t => t.Id == transaccionId)
                .FirstOrDefaultAsync();

            if (transaccion == null)
            {
                TempData["Error"] = "Comprobante no encontrado.";
                return RedirectToAction("EstadoCuenta");
            }

            var model = new ComprobanteDePagoViewModel
            {
                NumeroTransaccion = transaccion.NumeroTransaccion,
                Fecha = transaccion.Fecha,
                Monto = transaccion.Monto,
                Metodo = transaccion.MetodoPago, // Asegúrate de que "MetodoPago" esté en tu modelo de Transaccion
                Estado = transaccion.Estado
            };

            return View(model);
        }

        // Reenviar comprobante por email o WhatsApp
        public IActionResult ReenviarComprobante(string metodo, int transaccionId)
        {
            var transaccion = _db.Transacciones.Find(transaccionId);

            if (transaccion == null)
            {
                TempData["Error"] = "Comprobante no encontrado.";
                return RedirectToAction("EstadoCuenta");
            }

            // Lógica de envío por correo o WhatsApp
            if (metodo == "email")
            {
                // Aquí va el código para enviar el correo
                TempData["Success"] = "Comprobante de pago enviado por correo electrónico.";
            }
            else if (metodo == "whatsapp")
            {
                // Aquí va el código para enviar el mensaje por WhatsApp
                TempData["Success"] = "Comprobante de pago enviado por WhatsApp.";
            }

            return RedirectToAction("EstadoCuenta");
        }

        // Exportar historial de transacciones a PDF
        public IActionResult ExportarPdf()
        {
            var userId = User.Identity.Name;

            var cliente = _db.Clientes
                .Where(c => c.UserId == userId)
                .Include(c => c.Deuda)
                .FirstOrDefault();

            if (cliente == null || cliente.Deuda == null)
            {
                TempData["Error"] = "Cliente o deuda no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var historial = _db.Transacciones
                .Where(t => t.ClienteId == cliente.Id)
                .OrderByDescending(t => t.Fecha)  // Ordenar por fecha descendente
                .ToList();

            var pdfContent = GeneratePdfContent(historial);  // Llamar a una función que genere el contenido HTML para el PDF
            var pdfBytes = GeneratePdf(pdfContent);  // Generar el PDF usando alguna librería como DinkToPdf

            return File(pdfBytes, "application/pdf", "HistorialTransacciones.pdf");
        }

        // Función para generar el contenido HTML para el PDF
        private string GeneratePdfContent(List<Transaccion> historial)
        {
            var content = "<html><body><h1>Historial de Transacciones</h1><table>";

            foreach (var trans in historial)
            {
                content += $"<tr><td>{trans.Fecha}</td><td>{trans.Descripcion}</td><td>{trans.Monto}</td><td>{trans.Estado}</td></tr>";
            }

            content += "</table></body></html>";
            return content;
        }

        // Función para generar el PDF
        private byte[] GeneratePdf(string htmlContent)
        {
            var converter = new BasicConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return converter.Convert(doc);
        }
    }
}
