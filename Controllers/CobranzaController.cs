using Audicob.Data;
using Audicob.Models;
using Audicob.Models.ViewModels.Cobranza;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;


namespace Audicob.Controllers
{
    [Authorize(Roles = "AsesorCobranza")]
    public class CobranzaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CobranzaController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. Dashboard de Cobranza con búsqueda
        public async Task<IActionResult> Dashboard(string searchTerm = "")
        {
            try
            {
                var userId = User.Identity.Name;

                // Obtener todas las asignaciones del asesor
                var asignaciones = await _db.AsignacionesAsesores
                    .Include(a => a.Cliente)
                    .Where(a => a.AsesorUserId == userId)
                    .ToListAsync();

                // Filtrar clientes por nombre o documento
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    asignaciones = asignaciones.Where(a =>
                        a.Cliente.Nombre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        a.Cliente.Documento.Contains(searchTerm)).ToList();
                }

                // Crear el modelo de vista
                var vm = new CobranzaDashboardViewModel
                {
                    TotalClientesAsignados = asignaciones.Count,
                    TotalDeudaCartera = asignaciones.Sum(a => a.Cliente.DeudaTotal),
                    Clientes = asignaciones.Select(a => new ClienteDeudaViewModel
                    {
                        ClienteId = a.Cliente.Id,
                        ClienteNombre = a.Cliente.Nombre,
                        DeudaTotal = a.Cliente.DeudaTotal
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al cargar el dashboard: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // 2. Consultar Deuda Detallada
        public async Task<IActionResult> ConsultarDeuda(int clienteId)
        {
            try
            {
                var cliente = await _db.Clientes
                    .Include(c => c.Deuda)
                    .FirstOrDefaultAsync(c => c.Id == clienteId);

                if (cliente == null || cliente.Deuda == null)
                {
                    TempData["Error"] = "Cliente o deuda no encontrada.";
                    return RedirectToAction("Dashboard");
                }

                var deuda = cliente.Deuda;
                var diasDeAtraso = (DateTime.Now - deuda.FechaVencimiento).Days;
                var penalidadCalculada = CalcularPenalidad(deuda.Monto, diasDeAtraso);

                var model = new DeudaDetalleViewModel
                {
                    Cliente = cliente.Nombre,
                    MontoDeuda = deuda.Monto,
                    DiasAtraso = diasDeAtraso,
                    PenalidadCalculada = penalidadCalculada,
                    TotalAPagar = deuda.Monto + penalidadCalculada,
                    FechaVencimiento = deuda.FechaVencimiento,
                    TasaPenalidad = 0.015m
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al consultar la deuda: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        // 3. Actualizar Penalidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPenalidad(int clienteId)
        {
            try
            {
                var cliente = await _db.Clientes
                    .Include(c => c.Deuda)
                    .FirstOrDefaultAsync(c => c.Id == clienteId);

                if (cliente == null || cliente.Deuda == null)
                {
                    TempData["Error"] = "Cliente o deuda no encontrada.";
                    return RedirectToAction("Dashboard");
                }

                var deuda = cliente.Deuda;
                var diasDeAtraso = (DateTime.Now - deuda.FechaVencimiento).Days;
                var penalidadCalculada = CalcularPenalidad(deuda.Monto, diasDeAtraso);

                deuda.PenalidadCalculada = penalidadCalculada;
                deuda.TotalAPagar = deuda.Monto + penalidadCalculada;

                _db.Update(deuda);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Penalidad actualizada correctamente.";
                return RedirectToAction("ConsultarDeuda", new { clienteId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la penalidad: " + ex.Message;
                return RedirectToAction("ConsultarDeuda", new { clienteId });
            }
        }

        // 4. Generar Comprobante PDF
        public async Task<IActionResult> GenerarComprobante(int clienteId)
        {
            try
            {
                var cliente = await _db.Clientes
                    .Include(c => c.Deuda)
                    .FirstOrDefaultAsync(c => c.Id == clienteId);

                if (cliente == null || cliente.Deuda == null)
                {
                    TempData["Error"] = "Cliente o deuda no encontrada.";
                    return RedirectToAction("Dashboard");
                }

                var deuda = cliente.Deuda;
                var diasDeAtraso = (DateTime.Now - deuda.FechaVencimiento).Days;
                var penalidadCalculada = CalcularPenalidad(deuda.Monto, diasDeAtraso);

                var model = new ComprobanteDeudaViewModel
                {
                    Cliente = cliente.Nombre,
                    MontoDeuda = deuda.Monto,
                    DiasDeAtraso = diasDeAtraso,
                    TasaPenalidad = 0.015m,
                    PenalidadCalculada = penalidadCalculada,
                    TotalAPagar = deuda.Monto + penalidadCalculada,
                    FechaVencimiento = deuda.FechaVencimiento
                };

                var htmlContent = GenerateHtml(model);
                var pdfBytes = GeneratePdf(htmlContent);

                return File(pdfBytes, "application/pdf", "ComprobanteDeDeuda.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar el comprobante: " + ex.Message;
                return RedirectToAction("ConsultarDeuda", new { clienteId });
            }
        }

        // Calcular Penalidad
        private decimal CalcularPenalidad(decimal monto, int diasDeAtraso)
        {
            decimal tasaPenalidad = 0.015m; // Tasa de penalidad mensual (1.5%)
            return monto * tasaPenalidad * diasDeAtraso / 30;
        }

        // Generar HTML para el PDF
        private string GenerateHtml(ComprobanteDeudaViewModel model)
        {
            return $@"
                <html>
                    <body>
                        <h1>Comprobante de Deuda</h1>
                        <p><b>Cliente:</b> {model.Cliente}</p>
                        <p><b>Monto Original:</b> {model.MontoDeuda}</p>
                        <p><b>Días de Atraso:</b> {model.DiasDeAtraso}</p>
                        <p><b>Tasa de Penalidad:</b> {model.TasaPenalidad}%</p>
                        <p><b>Penalidad Calculada:</b> {model.PenalidadCalculada}</p>
                        <p><b>Total a Pagar:</b> {model.TotalAPagar}</p>
                    </body>
                </html>
            ";
        }

        // Función que genera el PDF (usando la librería DinkToPdf)
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
