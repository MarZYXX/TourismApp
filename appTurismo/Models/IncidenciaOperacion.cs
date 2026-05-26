namespace appTurismo.Models
{
    public class IncidenciaOperacion
    {
        public IncidenciaParticipante Incidencia { get; set; } = new();
        public string NombreTurista { get; set; } = string.Empty;
        public string NombreViaje { get; set; } = string.Empty;
        public string NombreCheckpoint { get; set; } = string.Empty;

        public string TipoVisible => Incidencia.Tipo.Replace('_', ' ');
        public string EstadoVisible => Incidencia.Estado;
        public bool EsPrioritaria => Incidencia.RequiereAtencion;
        public bool EsCerrada => string.Equals(Incidencia.Estado, "Cerrada", StringComparison.OrdinalIgnoreCase);
        public string FechaVisible => Incidencia.CreatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
    }
}
