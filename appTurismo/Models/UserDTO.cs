using CommunityToolkit.Mvvm.ComponentModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace appTurismo.Models;

[Table("usuarios")]
public partial class UserDTO : ObservableObject
{
    public Guid IdUsuario { get; set; }

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _apellidoPaterno = string.Empty;

    [ObservableProperty]
    private string _apellidoMaterno = string.Empty;

    [ObservableProperty]
    private string _correoElectronico = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    public Guid IdRol { get; set; }

    public double? UltimaLatitud { get; set; }

    public double? UltimaLongitud { get; set; }

    public DateTime? UltimaActualizacion { get; set; }

    public DateTime CreatedAt { get; set; }
}
