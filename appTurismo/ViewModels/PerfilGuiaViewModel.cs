using System.Windows.Input;
using appTurismo.Services;
using appTurismo.Helpers;

namespace appTurismo.ViewModels
{
    public class PerfilGuiaViewModel : GuiaBaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IViajeService _viajeService;
        private Models.Supabase.User? _perfil;
        private bool _estaEditando;
        private string _nombre = string.Empty;
        private string _apellidoPaterno = string.Empty;
        private string _apellidoMaterno = string.Empty;
        private string _telefono = string.Empty;
        private int _viajesPlanificados;
        private int _viajesEnCurso;
        private int _viajesFinalizados;
        private int _viajesCancelados;

        public Models.Supabase.User? Perfil
        {
            get => _perfil;
            private set
            {
                _perfil = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TienePerfil));
                OnPropertyChanged(nameof(NombreCompleto));
            }
        }

        public bool TienePerfil => Perfil != null;
        public string NombreCompleto => Perfil == null
            ? string.Empty
            : $"{Perfil.Nombre} {Perfil.Apellido_paterno} {Perfil.Apellido_materno}".Trim();
        public string RolVisible => "Guía";

        public bool EstaEditando
        {
            get => _estaEditando;
            private set
            {
                _estaEditando = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstaConsultando));
            }
        }

        public bool EstaConsultando => !EstaEditando;
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        public string ApellidoPaterno { get => _apellidoPaterno; set { _apellidoPaterno = value; OnPropertyChanged(); } }
        public string ApellidoMaterno { get => _apellidoMaterno; set { _apellidoMaterno = value; OnPropertyChanged(); } }
        public string Telefono { get => _telefono; set { _telefono = value; OnPropertyChanged(); } }

        public int ViajesPlanificados { get => _viajesPlanificados; private set { _viajesPlanificados = value; OnPropertyChanged(); } }
        public int ViajesEnCurso { get => _viajesEnCurso; private set { _viajesEnCurso = value; OnPropertyChanged(); } }
        public int ViajesFinalizados { get => _viajesFinalizados; private set { _viajesFinalizados = value; OnPropertyChanged(); } }
        public int ViajesCancelados { get => _viajesCancelados; private set { _viajesCancelados = value; OnPropertyChanged(); } }
        public int TotalViajes => ViajesPlanificados + ViajesEnCurso + ViajesFinalizados + ViajesCancelados;

        public ICommand CargarPerfilCommand { get; }
        public ICommand EditarPerfilCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand GuardarPerfilCommand { get; }

        public PerfilGuiaViewModel(
            IUserService userService,
            IViajeService viajeService) : base(userService)
        {
            _userService = userService;
            _viajeService = viajeService;
            Title = "Perfil";

            CargarPerfilCommand = new Command(async () => await CargarAsync());
            EditarPerfilCommand = new Command(IniciarEdicion);
            CancelarEdicionCommand = new Command(CancelarEdicion);
            GuardarPerfilCommand = new Command(async () => await GuardarAsync());
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Perfil = await _userService.GetCurrentUserProfileAsync();
                if (Perfil == null)
                {
                    await Shell.Current.DisplayAlertAsync("Perfil no disponible", "No fue posible obtener tus datos de guía.", "OK");
                    return;
                }

                CopiarCamposEdicion();
                var viajes = await _viajeService.GetGuideTripsAsync();
                ViajesPlanificados = viajes.Count(v => EsEstado(v.Estado, "Plan"));
                ViajesEnCurso = viajes.Count(v => EsEstado(v.Estado, "Activo"));
                ViajesFinalizados = viajes.Count(v => EsEstado(v.Estado, "Completado"));
                ViajesCancelados = viajes.Count(v => EsEstado(v.Estado, "Cancelado"));
                OnPropertyChanged(nameof(TotalViajes));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar perfil: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible cargar el perfil.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void IniciarEdicion()
        {
            if (Perfil == null) return;
            CopiarCamposEdicion();
            EstaEditando = true;
        }

        private void CancelarEdicion()
        {
            CopiarCamposEdicion();
            EstaEditando = false;
        }

        private async Task GuardarAsync()
        {
            if (Perfil == null || IsBusy) return;

            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(ApellidoPaterno))
            {
                await Shell.Current.DisplayAlertAsync("Datos incompletos", "El nombre y apellido paterno son obligatorios.", "OK");
                return;
            }

            if (!FormValidators.IsValidName(Nombre) ||
                !FormValidators.IsValidName(ApellidoPaterno) ||
                !FormValidators.IsValidName(ApellidoMaterno, required: false))
            {
                await Shell.Current.DisplayAlertAsync("Nombre inválido", "Nombre y apellidos solo pueden incluir letras, espacios, guion o apóstrofo.", "OK");
                return;
            }

            if (!FormValidators.IsValidPhone(Telefono))
            {
                await Shell.Current.DisplayAlertAsync("Teléfono inválido", "Ingresa exactamente 10 dígitos, sin espacios ni guiones.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                await _userService.UpdateCurrentUserProfileAsync(
                    Nombre.Trim(),
                    ApellidoPaterno.Trim(),
                    ApellidoMaterno.Trim(),
                    Telefono.Trim());

                Perfil.Nombre = Nombre.Trim();
                Perfil.Apellido_paterno = ApellidoPaterno.Trim();
                Perfil.Apellido_materno = ApellidoMaterno.Trim();
                Perfil.Telefono = Telefono.Trim();
                OnPropertyChanged(nameof(Perfil));
                OnPropertyChanged(nameof(NombreCompleto));
                EstaEditando = false;
                await Shell.Current.DisplayAlertAsync("Perfil actualizado", "Tus datos fueron guardados correctamente.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar perfil: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo actualizar", "Supabase no permitió actualizar el perfil del guía.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CopiarCamposEdicion()
        {
            if (Perfil == null) return;

            Nombre = Perfil.Nombre;
            ApellidoPaterno = Perfil.Apellido_paterno;
            ApellidoMaterno = Perfil.Apellido_materno;
            Telefono = Perfil.Telefono;
        }

        private static bool EsEstado(string estadoActual, string estadoBuscado) =>
            string.Equals(estadoActual, estadoBuscado, StringComparison.OrdinalIgnoreCase);
    }
}
