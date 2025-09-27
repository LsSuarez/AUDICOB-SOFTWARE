using Audicob.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Audicob.Data.SeedData
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Crear roles si no existen
            string[] roles = { "Administrador", "Supervisor", "AsesorCobranza", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var identityRole = new IdentityRole(role);
                    var roleResult = await roleManager.CreateAsync(identityRole);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            Console.WriteLine($"Error al crear el rol {role}: {error.Description}");
                        }
                    }
                }
            }

            // Crear usuarios de prueba si no existen
            await CreateUserAsync(userManager, "admin@audicob.com", "Admin123!", "Administrador", "Administrador General");
            await CreateUserAsync(userManager, "supervisor@audicob.com", "Supervisor123!", "Supervisor", "Supervisor Principal");
            await CreateUserAsync(userManager, "asesor@audicob.com", "Asesor123!", "AsesorCobranza", "Asesor de Cobranza");
            await CreateUserAsync(userManager, "cliente@audicob.com", "Cliente123!", "Cliente", "Cliente Demo");

            // Insertar clientes de ejemplo si no existen
            if (!await db.Clientes.AnyAsync())
            {
                var cliente1 = new Cliente
                {
                    Documento = "12345678",
                    Nombre = "Juan Pérez",
                    IngresosMensuales = 2500,
                    DeudaTotal = 1200
                };

                var cliente2 = new Cliente
                {
                    Documento = "87654321",
                    Nombre = "María López",
                    IngresosMensuales = 3200,
                    DeudaTotal = 800
                };

                db.Clientes.AddRange(cliente1, cliente2);
                await db.SaveChangesAsync();

                // Insertar pagos de ejemplo
                db.Pagos.AddRange(
                    new Pago { ClienteId = cliente1.Id, Fecha = DateTime.UtcNow.AddMonths(-1), Monto = 200, Validado = true },
                    new Pago { ClienteId = cliente1.Id, Fecha = DateTime.UtcNow.AddMonths(-3), Monto = 150, Validado = false },
                    new Pago { ClienteId = cliente2.Id, Fecha = DateTime.UtcNow.AddMonths(-2), Monto = 300, Validado = true }
                );

                // Insertar evaluaciones de ejemplo
                db.Evaluaciones.AddRange(
                    new EvaluacionCliente { ClienteId = cliente1.Id, Estado = "Pendiente" },
                    new EvaluacionCliente { ClienteId = cliente2.Id, Estado = "Pendiente" }
                );

                await db.SaveChangesAsync();
            }
        }

        private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role, string fullName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    // Manejo de errores, en caso de que la creación del usuario falle
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error al crear el usuario {email}: {error.Description}");
                    }
                }
            }
        }
    }
}
