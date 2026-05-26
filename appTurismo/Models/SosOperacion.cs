namespace appTurismo.Models
{
    public class SosOperacion
    {
        public SosSolicitud Solicitud { get; set; } = new();
        public string NombreTurista { get; set; } = string.Empty;
        public string NombreViaje { get; set; } = string.Empty;
        public bool EsResuelta =>
            string.Equals(Solicitud.Estado, "Resuelto", StringComparison.OrdinalIgnoreCase);

        public string FechaVisible =>
            Solicitud.Timestamp?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;

        public string UbicacionVisible =>
            $"{Solicitud.Latitud:F5}, {Solicitud.Longitud:F5}";
    }
}
