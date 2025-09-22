using System;

namespace PROYECTO_AUDICOB.Models
{
    public class Evaluacion
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public decimal Ingresos { get; set; }              // numeric
        public decimal Deudas { get; set; }                // numeric
        public string HistorialPagosJson { get; set; }     // text
        public int Estado { get; set; }                    // int4
        public string Comentario { get; set; }             // text
        public string UsuarioResponsableId { get; set; }   // text
        public DateTime FechaRegistro { get; set; }        // timestamptz
        public DateTime? FechaRevision { get; set; }       // timestamptz nullable
    }
}

