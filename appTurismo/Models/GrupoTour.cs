using System;
using System.Collections.Generic;

namespace appTurismo.Models
{
    public class GrupoTour
    {
        public string IdTourGroup { get; set; } = string.Empty;
        public string? IdPaquete { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string GuiaId { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string? Descripcion { get; set; }
        public string? PuntoEncuentro { get; set; }
        public int? CupoMaximo { get; set; }
    }
}
