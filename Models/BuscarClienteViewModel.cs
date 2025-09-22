using System.Collections.Generic;
using PROYECTO_AUDICOB.Models;

namespace PROYECTO_AUDICOB.ViewModels
{
    public class BuscarClienteViewModel
    {
        public string Dni { get; set; }               
        public string NombreCliente { get; set; }     
        public string Telefono { get; set; }          
        public List<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
    }
}