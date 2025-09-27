using Audicob.Models;
using Audicob.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Audicob.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, 
                                 SignInManager<ApplicationUser> signInManager, 
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Vista Login
        public IActionResult Login() => View();

        // Procesar Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model); // Si los datos no son válidos, devuelve la vista con los errores

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(model); // Si las credenciales son incorrectas, muestra el error
            }

            // Obtener el usuario y sus roles
            var user = await _userManager.FindByEmailAsync(model.Email);
            var roles = await _userManager.GetRolesAsync(user);

            // Registrar la actividad de inicio de sesión
            _logger.LogInformation($"Usuario {user.Email} ha iniciado sesión correctamente.");

            // Redirigir según el rol
            if (roles.Contains("Administrador"))
                return RedirectToAction("Dashboard", "Admin");
            if (roles.Contains("Supervisor"))
                return RedirectToAction("Dashboard", "Supervisor");
            if (roles.Contains("AsesorCobranza"))
                return RedirectToAction("Dashboard", "Asesor");
            if (roles.Contains("Cliente"))
                return RedirectToAction("Dashboard", "Cliente");

            // Si no se encuentra un rol adecuado, redirigir al Home
            return RedirectToAction("Index", "Home");
        }

        // Vista Registro
        public IActionResult Register() => View();

        // Procesar Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model); // Si los datos no son válidos, devuelve la vista con los errores

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
                return View(model); // Si las contraseñas no coinciden, muestra el error
            }

            // Crear el nuevo usuario
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true // Si decides usar confirmación de correo, cambia esto a false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model); // Si ocurre un error, muestra el error en el formulario
            }

            // Asignar el rol
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model); // Si la asignación del rol falla, muestra el error
                }
            }

            _logger.LogInformation($"Nuevo usuario registrado: {user.Email}");

            TempData["Success"] = "¡Registro exitoso! Ahora puedes iniciar sesión."; // Mensaje de éxito
            return RedirectToAction("Login"); // Redirigir al login después de un registro exitoso
        }

        // Cerrar sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Cerrar sesión
            _logger.LogInformation("El usuario ha cerrado sesión.");
            return RedirectToAction("Index", "Home"); // Redirigir al home después de cerrar sesión
        }

        // Acceso denegado
        public IActionResult AccessDenied() => View(); // Vista de acceso denegado
    }
}
