using System.Collections.Generic;

namespace PROYECTO_AUDICOB.Models
{
    public class Cliente
    {
        public int Id { get; set; }           // PK
        public string Dni { get; set; }       // DNI del cliente
        public string Nombre { get; set; }    // Nombre completo
        public string Telefono { get; set; }  // Teléfono

        // Relación 1:N con Evaluaciones
        public List<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
    }
}
