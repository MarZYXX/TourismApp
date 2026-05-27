using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models
{
    [Table("checkpoints")]
    public class Checkpoint : BaseModel
    {
        [PrimaryKey("id_checkpoint", true)]
        public string IdCheckpoint { get; set; } = string.Empty;

        [Column("id_grupo")]
        public string IdGrupo { get; set; } = string.Empty;

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("latitud")]
        public double Latitud { get; set; }

        [Column("longitud")]
        public double Longitud { get; set; }

        [Column("completado")]
        public bool Completado { get; set; }

        [Column("orden")]
        public int Orden { get; set; }
    }
}
