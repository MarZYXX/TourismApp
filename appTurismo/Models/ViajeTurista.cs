namespace appTurismo.Models
{
    public class ViajeTurista
    {
        public GrupoTour Viaje { get; set; } = new();
        public string EstadoParticipacion { get; set; } = "Activo";
        public string NombreGuia { get; set; } = string.Empty;
        public string TelefonoGuia { get; set; } = string.Empty;
        public int TotalCheckpoints { get; set; }
        public int CheckpointsCompletados { get; set; }

        public string IdTourGroup => Viaje.IdTourGroup;
        public string Nombre => Viaje.Nombre;
        public string Estado => Viaje.Estado;
        public DateTime FechaInicio => Viaje.FechaInicio;
        public string? Descripcion => Viaje.Descripcion;
        public string? PuntoEncuentro => Viaje.PuntoEncuentro;
        public string ProgresoTexto => TotalCheckpoints == 0
            ? "Ruta sin checkpoints"
            : $"{CheckpointsCompletados} de {TotalCheckpoints} checkpoints completados";
    }
}
