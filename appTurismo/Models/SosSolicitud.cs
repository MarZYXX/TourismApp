using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models
{
    [Table("sos")]
    public class SosSolicitud : BaseModel
    {
        [PrimaryKey("id_sos", true)]
        public string IdSos { get; set; } = string.Empty;

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("id_grupo_tour")]
        public string IdGrupoTour { get; set; } = string.Empty;

        [Column("id_checkpoint")]
        public string? IdCheckpoint { get; set; }

        [Column("latitud")]
        public double Latitud { get; set; }

        [Column("longitud")]
        public double Longitud { get; set; }

        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Activo";

        [Column("guia_id")]
        public string GuiaId { get; set; } = string.Empty;
    }
}
