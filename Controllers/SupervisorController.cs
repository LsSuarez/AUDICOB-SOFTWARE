using Audicob.Data;
using Audicob.Models;
using Audicob.Models.ViewModels.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Audicob.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        // Dashboard principal
        public async Task<IActionResult> Dashboard()
        {
            var vm = new SupervisorDashboardViewModel
            {
                // Datos de resumen
                TotalClientes = await _db.Clientes.CountAsync(),
                EvaluacionesPendientes = await _db.Evaluaciones.CountAsync(e => e.Estado == "Pendiente"),
                TotalDeuda = await _db.Clientes.SumAsync(c => c.DeudaTotal),
                TotalPagosUltimoMes = await _db.Pagos
                    .Where(p => p.Fecha >= DateTime.UtcNow.AddMonths(-1))
                    .SumAsync(p => p.Monto)
            };

            // Obtener pagos de los últimos 6 meses
            var pagos = await _db.Pagos
                .Where(p => p.Fecha >= DateTime.UtcNow.AddMonths(-6))
                .GroupBy(p => new { p.Fecha.Year, p.Fecha.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.Monto)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Realizamos el formateo en memoria
            var pagosFormat = pagos.Select(g => new
            {
                Mes = $"{g.Month}/{g.Year}",  // Formateamos el mes y el año
                Total = g.Total
            }).ToList();

            // Asignamos los resultados a las propiedades del modelo
            vm.Meses = pagosFormat.Select(p => p.Mes).ToList();
            vm.PagosPorMes = pagosFormat.Select(p => p.Total).ToList();

            // Obtener las 5 mayores deudas de los clientes
            var deudas = await _db.Clientes
                .OrderByDescending(c => c.DeudaTotal)
                .Take(5)
                .Select(c => new { c.Nombre, c.DeudaTotal })
                .ToListAsync();

            // Asignamos los resultados a las propiedades correspondientes en el modelo
            vm.Clientes = deudas.Select(d => d.Nombre).ToList();
            vm.DeudasPorCliente = deudas.Select(d => d.DeudaTotal).ToList();

            // Obtener los pagos pendientes (por ejemplo, clientes con pagos pendientes)
            var pagosPendientes = await _db.Pagos
                .Where(p => p.Estado == "Pendiente")
                .Include(p => p.Cliente) // Asegúrate de incluir el cliente
                .Take(5)
                .ToListAsync();

            // Asignar los pagos pendientes a la vista
            vm.PagosPendientes = pagosPendientes;

            return View(vm);
        }

        // Acción para validar un pago
        [HttpPost]
        public async Task<IActionResult> ValidarPago(int pagoId)
        {
            // Buscar el pago por su ID
            var pago = await _db.Pagos.FirstOrDefaultAsync(p => p.Id == pagoId);
            if (pago == null)
            {
                TempData["Error"] = "Pago no encontrado.";
                return RedirectToAction("Dashboard");
            }

            // Si el pago está pendiente, lo validamos (lo marcamos como cancelado)
            if (pago.Estado == "Pendiente")
            {
                pago.Validado = true;
                pago.Estado = "Cancelado"; // Actualizar el estado a "Cancelado"
                _db.Update(pago);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Pago validado correctamente.";
            }
            else
            {
                TempData["Error"] = "El pago ya está validado o cancelado.";
            }

            return RedirectToAction("Dashboard");
        }

        // Acción para asignar una línea de crédito
        public async Task<IActionResult> AsignarLineaCredito(int id)
        {
            var cliente = await TryGetClienteAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Dashboard");
            }

            if (cliente.LineaCredito != null)
            {
                TempData["Error"] = "Este cliente ya tiene una línea de crédito asignada.";
                return RedirectToAction("Dashboard");
            }

            var vm = new AsignacionLineaCreditoViewModel
            {
                ClienteId = cliente.Id,
                NombreCliente = cliente.Nombre,
                DeudaTotal = cliente.DeudaTotal,
                IngresosMensuales = cliente.IngresosMensuales
            };

            return View(vm);
        }

        // POST: Asignar línea de crédito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarLineaCredito(AsignacionLineaCreditoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cliente = await TryGetClienteAsync(model.ClienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente no válido.";
                return RedirectToAction("Dashboard");
            }

            if (cliente.LineaCredito != null)
            {
                TempData["Error"] = "Este cliente ya tiene una línea de crédito asignada.";
                return RedirectToAction("Dashboard");
            }

            var user = await _userManager.GetUserAsync(User);

            var linea = new LineaCredito
            {
                ClienteId = cliente.Id,
                Monto = model.MontoAsignado,
                FechaAsignacion = DateTime.UtcNow,
                UsuarioAsignador = user.FullName
            };

            _db.LineasCredito.Add(linea);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Línea de crédito asignada a {cliente.Nombre} por S/ {model.MontoAsignado:N2}.";
            return RedirectToAction("Dashboard");
        }

        // Método privado para obtener cliente con línea de crédito
        private async Task<Cliente?> TryGetClienteAsync(int clienteId)
        {
            return await _db.Clientes
                .Include(c => c.LineaCredito)
                .FirstOrDefaultAsync(c => c.Id == clienteId);
        }
    }
}
