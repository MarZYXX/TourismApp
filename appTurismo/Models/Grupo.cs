using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace appTurismo.Models
{
    [Table("grupos")]
    public class Grupo : BaseModel
    {
        [PrimaryKey("id_tour_group", true)]
        public string IdTourGroup { get; set; } = string.Empty;

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("id_paquete")]
        public string? IdPaquete { get; set; }

        [Column("guia_id")]
        public string GuiaId { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("punto_encuentro")]
        public string? PuntoEncuentro { get; set; }

        [Column("cupo_maximo")]
        public int? CupoMaximo { get; set; }
    }
}
