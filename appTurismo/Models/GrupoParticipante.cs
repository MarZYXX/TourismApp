using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models
{
    [Table("grupo_participantes")]
    public class GrupoParticipante : BaseModel
    {
        [PrimaryKey("id_grupo", true)]
        public string IdGrupo { get; set; } = string.Empty;

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("joined_at")]
        public DateTime? JoinedAt { get; set; }
    }
}
