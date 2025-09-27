using Audicob.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Audicob.Data.Configurations;

namespace Audicob.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas personalizadas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<LineaCredito> LineasCredito { get; set; }
        public DbSet<EvaluacionCliente> Evaluaciones { get; set; }
        public DbSet<AsignacionAsesor> AsignacionesAsesores { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones Fluent API
            modelBuilder.ApplyConfiguration(new ClienteConfiguration());
            modelBuilder.ApplyConfiguration(new PagoConfiguration());
            modelBuilder.ApplyConfiguration(new LineaCreditoConfiguration());
            modelBuilder.ApplyConfiguration(new EvaluacionClienteConfiguration());
            modelBuilder.ApplyConfiguration(new AsignacionAsesorConfiguration());
        }
    }
}
