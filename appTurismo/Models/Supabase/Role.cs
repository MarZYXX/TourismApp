using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace appTurismo.Models.Supabase
{
    [Table("roles")]
    public class Role : BaseModel
    {
        [PrimaryKey("id_rol", false)]
        public Guid Id_rol { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }
    }
}