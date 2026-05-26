namespace appTurismo.Models
{
    public class ViajeTurista
    {
        public GrupoTour Viaje { get; set; } = new();
        public string EstadoParticipacion { get; set; } = "Activo";
        public string ConfirmacionAsistencia { get; set; } = "Pendiente";
        public string NombreGuia { get; set; } = string.Empty;
        public string TelefonoGuia { get; set; } = string.Empty;
        public int TotalCheckpoints { get; set; }
        public int CheckpointsCompletados { get; set; }
        public SosSolicitud? SosActivo { get; set; }
        public SosSolicitud? UltimoSosResuelto { get; set; }

        public string IdTourGroup => Viaje.IdTourGroup;
        public string Nombre => Viaje.Nombre;
        public string Estado => Viaje.Estado;
        public DateTime FechaInicio => Viaje.FechaInicio;
        public string? Descripcion => Viaje.Descripcion;
        public string? PuntoEncuentro => Viaje.PuntoEncuentro;
        public string ConfirmacionTexto => ConfirmacionAsistencia switch
        {
            "Confirmado" => "Asistencia confirmada",
            "No_asistira" => "No asistirás",
            _ => "Confirmación pendiente"
        };
        public bool TieneSosActivo => SosActivo != null;
        public bool TieneSosResuelto => UltimoSosResuelto != null;
        public bool PuedeEnviarSos => SosActivo == null;
        public string SosEstadoTexto => SosActivo == null
            ? "Usa este boton solo si necesitas ayuda durante el recorrido."
            : "Solicitud SOS enviada. El guía ha sido notificado.";
        public string UltimoSosResueltoTexto => UltimoSosResuelto?.Timestamp?.ToLocalTime()
            .ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
        public string ProgresoTexto => TotalCheckpoints == 0
            ? "Ruta sin checkpoints"
            : $"{CheckpointsCompletados} de {TotalCheckpoints} checkpoints completados";
    }
}
