namespace appTurismo.Models
{
    public class ParticipanteViaje
    {
        public Models.Supabase.User Usuario { get; set; } = new();
        public string ConfirmacionAsistencia { get; set; } = "Pendiente";

        public string NombreCompleto =>
            $"{Usuario.Nombre} {Usuario.Apellido_paterno}".Trim();

        public string CorreoElectronico => Usuario.Correo_electronico;

        public string ConfirmacionTexto => ConfirmacionAsistencia switch
        {
            "Confirmado" => "Confirmado",
            "No_asistira" => "No asistirá",
            _ => "Pendiente"
        };
    }
}
