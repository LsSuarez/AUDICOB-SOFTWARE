using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Para Include
using Microsoft.Extensions.Logging;
using PROYECTO_AUDICOB.Models;
using PROYECTO_AUDICOB.ViewModels;
using PROYECTO_AUDICOB.Data;

namespace AUDICOB_SOFTWARE.Controllers
{
    public class EvaluacionController : Controller
    {
        private readonly ILogger<EvaluacionController> _logger;
        private readonly ApplicationDbContext _context;

        public EvaluacionController(ILogger<EvaluacionController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Evaluacion/Buscar
        [HttpGet]
        public IActionResult Buscar()
        {
            ViewData["HideNav"] = true; // Oculta menú en GET
            return View(new BuscarClienteViewModel());
        }

        // POST: /Evaluacion/Buscar
        [HttpPost]
        public IActionResult Buscar(BuscarClienteViewModel model)
        {
            // IMPORTANTE: ponerlo también aquí
            ViewData["HideNav"] = true; // Oculta menú en POST

            var cliente = _context.Clientes
                .Include(c => c.Evaluaciones)
                .FirstOrDefault(c => c.Dni == model.Dni);

            if (cliente != null)
            {
                // Limpiar errores previos
                ModelState.Clear();

                model.NombreCliente = cliente.Nombre;
                model.Telefono = cliente.Telefono;
                model.Evaluaciones = cliente.Evaluaciones
                    .OrderByDescending(e => e.FechaRegistro)
                    .ToList();
            }
            else
            {
                model.Evaluaciones = new List<Evaluacion>();
                ModelState.AddModelError("", "Cliente no encontrado");
            }

            return View(model);
        }
    }
}
