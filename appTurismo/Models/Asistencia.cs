using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models
{
    [Table("asistencia")]
    public class Asistencia : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        [Column("id_checkpoint")]
        public string IdCheckpoint { get; set; } = string.Empty;

        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("presente")]
        public bool Presente { get; set; }
    }
}
