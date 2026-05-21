using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models.Supabase
{
    [Table("usuarios")]
    public class User : BaseModel
    {
        [PrimaryKey("id_usuario", true)]
        public Guid Id_usuario { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = "";

        [Column("apellido_paterno")]
        public string Apellido_paterno { get; set; } = "";

        [Column("apellido_materno")]
        public string Apellido_materno { get; set; } = "";

        [Column("correo_electronico")]
        public string Correo_electronico { get; set; } = "";

        [Column("telefono")]
        public string Telefono { get; set; } = "";

        [Column("id_rol")]
        public Guid Id_rol { get; set; }

        [Column("ultima_latitud")]
        public double? Ultima_latitud { get; set; }

        [Column("ultima_longitud")]
        public double? Ultima_longitud { get; set; }

        [Column("ultima_actualizacion")]
        public DateTime? Ultima_actualizacion { get; set; }

        [Column("created_at")]
        public DateTime Created_at { get; set; }
    }
}