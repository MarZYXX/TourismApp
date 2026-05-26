namespace appTurismo.Models
{
    public class ViajeCatalogo
    {
        public GrupoTour Viaje { get; set; } = new();
        public string NombreGuia { get; set; } = "Guía por confirmar";
        public int ParticipantesInscritos { get; set; }

        public string IdTourGroup => Viaje.IdTourGroup;
        public string Nombre => Viaje.Nombre;
        public DateTime FechaInicio => Viaje.FechaInicio;
        public string? Descripcion => Viaje.Descripcion;
        public string? PuntoEncuentro => Viaje.PuntoEncuentro;
        public int? CupoMaximo => Viaje.CupoMaximo;
        public int? LugaresDisponibles => CupoMaximo.HasValue
            ? Math.Max(CupoMaximo.Value - ParticipantesInscritos, 0)
            : null;
        public bool TieneCupo => !LugaresDisponibles.HasValue || LugaresDisponibles.Value > 0;
        public string CupoTexto => LugaresDisponibles.HasValue
            ? $"{LugaresDisponibles.Value} lugar(es) disponible(s)"
            : "Cupo disponible";
    }
}
