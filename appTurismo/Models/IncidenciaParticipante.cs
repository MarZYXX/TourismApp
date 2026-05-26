using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models
{
    [Table("incidencias_participante")]
    public class IncidenciaParticipante : BaseModel
    {
        [PrimaryKey("id_incidencia", true)]
        public string IdIncidencia { get; set; } = string.Empty;

        [Column("id_grupo")]
        public string IdGrupo { get; set; } = string.Empty;

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("id_checkpoint")]
        public string? IdCheckpoint { get; set; }

        [Column("id_guia")]
        public string IdGuia { get; set; } = string.Empty;

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Abierta";

        [Column("requiere_atencion")]
        public bool RequiereAtencion { get; set; }

        [Column("latitud")]
        public double? Latitud { get; set; }

        [Column("longitud")]
        public double? Longitud { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("nota_resolucion")]
        public string? NotaResolucion { get; set; }

        [Column("atendida_at")]
        public DateTime? AtendidaAt { get; set; }

        [Column("cerrada_at")]
        public DateTime? CerradaAt { get; set; }
    }
}
