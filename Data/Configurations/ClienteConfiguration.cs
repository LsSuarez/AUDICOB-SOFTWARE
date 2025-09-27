using Audicob.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audicob.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Documento)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.IngresosMensuales)
                .HasColumnType("numeric")
                .IsRequired();

            builder.Property(c => c.DeudaTotal)
                .HasColumnType("numeric")
                .IsRequired();

            builder.Property(c => c.FechaActualizacion)
                .IsRequired();

            // Relación: Cliente tiene muchos Pagos
            builder.HasMany(c => c.Pagos)
                .WithOne(p => p.Cliente)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación: Cliente tiene muchas Evaluaciones
            builder.HasMany(c => c.Evaluaciones)
                .WithOne(e => e.Cliente)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación: Cliente tiene una Línea de Crédito
            builder.HasOne(c => c.LineaCredito)
                .WithOne(l => l.Cliente)
                .HasForeignKey<LineaCredito>(l => l.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación: Cliente tiene una Asignación de Asesor
            builder.HasOne(c => c.AsignacionAsesor)
                .WithOne(a => a.Cliente)
                .HasForeignKey<AsignacionAsesor>(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
