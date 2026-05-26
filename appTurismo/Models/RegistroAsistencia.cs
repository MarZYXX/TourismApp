namespace appTurismo.Models
{
    public class RegistroAsistencia
    {
        public Models.Supabase.User Usuario { get; set; } = new();
        public bool Presente { get; set; }
        public string EstadoParticipante { get; set; } = "Activo";

        public string NombreCompleto =>
            $"{Usuario.Nombre} {Usuario.Apellido_paterno}".Trim();
    }
}
