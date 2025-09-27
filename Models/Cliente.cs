namespace Audicob.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        // Documento no puede ser nulo, por lo que se inicializa con string vacío
        public string Documento { get; set; } = string.Empty;

        // Nombre no puede ser nulo, por lo que se inicializa con string vacío
        public string Nombre { get; set; } = string.Empty;

        public decimal IngresosMensuales { get; set; }

        public decimal DeudaTotal { get; set; }

        // FechaActualizacion no puede ser nula, por lo que se inicializa con la fecha actual
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Relación con Pagos, siempre inicializado como colección vacía
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // Relación con Línea de Crédito, puede ser nulo
        public LineaCredito? LineaCredito { get; set; }

        // Relación con Evaluaciones, siempre inicializado como colección vacía
        public ICollection<EvaluacionCliente> Evaluaciones { get; set; } = new List<EvaluacionCliente>();

        // Relación con Asignación de Asesor, puede ser nulo
        public AsignacionAsesor? AsignacionAsesor { get; set; }

        // Relación con Deuda, la propiedad puede ser nula (indicado con '?')
        public Deuda? Deuda { get; set; }  // La propiedad de deuda es opcional (puede ser nula)

        // Nueva propiedad UserId, que se utilizará para la relación con el usuario
        public string UserId { get; set; } = string.Empty;  // Agregado para poder acceder al UserId
    }
}
