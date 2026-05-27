using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class TipoIncidenciaOption
    {
        public string Valor { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
    }

    public class RegistrarIncidenciaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private string _grupoId = string.Empty;
        private string _checkpointId = string.Empty;
        private Guid _turistaId;
        private string _turistaNombre = string.Empty;
        private string _checkpointNombre = string.Empty;
        private TipoIncidenciaOption? _tipoSeleccionado;
        private string _descripcion = string.Empty;
        private bool _requiereAtencion;
        private bool _retirarParticipante;

        public ObservableCollection<TipoIncidenciaOption> Tipos { get; } =
        [
            new() { Valor = "No_desea_continuar", Etiqueta = "No desea continuar" },
            new() { Valor = "Regreso_anticipado", Etiqueta = "Regreso anticipado" },
            new() { Valor = "Malestar", Etiqueta = "Malestar" },
            new() { Valor = "Accidente", Etiqueta = "Accidente" },
            new() { Valor = "Extraviado", Etiqueta = "Extraviado" },
            new() { Valor = "Otro", Etiqueta = "Otro" }
        ];

        public string TuristaNombre
        {
            get => _turistaNombre;
            private set { _turistaNombre = value; OnPropertyChanged(); }
        }

        public string CheckpointNombre
        {
            get => _checkpointNombre;
            private set { _checkpointNombre = value; OnPropertyChanged(); }
        }

        public TipoIncidenciaOption? TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set
            {
                _tipoSeleccionado = value;
                OnPropertyChanged();

                if (value?.Valor is "Accidente" or "Extraviado")
                {
                    RequiereAtencion = true;
                }

                if (value?.Valor is "No_desea_continuar" or "Regreso_anticipado")
                {
                    RetirarParticipante = true;
                }
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(); }
        }

        public bool RequiereAtencion
        {
            get => _requiereAtencion;
            set { _requiereAtencion = value; OnPropertyChanged(); }
        }

        public bool RetirarParticipante
        {
            get => _retirarParticipante;
            set { _retirarParticipante = value; OnPropertyChanged(); }
        }

        public ICommand GuardarIncidenciaCommand { get; }

        public RegistrarIncidenciaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Registrar incidencia";
            GuardarIncidenciaCommand = new Command(async () => await GuardarAsync());
        }

        public void Preparar(string grupoId, string checkpointId, string checkpointNombre, string turistaId, string turistaNombre)
        {
            _grupoId = grupoId;
            _checkpointId = checkpointId;
            Guid.TryParse(turistaId, out _turistaId);
            TuristaNombre = turistaNombre;
            CheckpointNombre = checkpointNombre;
        }

        private async Task GuardarAsync()
        {
            if (TipoSeleccionado == null)
            {
                await Shell.Current.DisplayAlertAsync("Falta tipo", "Selecciona el tipo de incidencia.", "OK");
                return;
            }

            if (_turistaId == Guid.Empty || string.IsNullOrWhiteSpace(_grupoId))
            {
                await Shell.Current.DisplayAlertAsync("Error", "No se encontró el participante del recorrido.", "OK");
                return;
            }

            double? latitud = null;
            double? longitud = null;

            try
            {
                var ubicacion = await Geolocation.Default.GetLastKnownLocationAsync();
                latitud = ubicacion?.Latitude;
                longitud = ubicacion?.Longitude;
            }
            catch
            {
            }

            try
            {
                IsBusy = true;
                await _viajeService.RegisterIncidentAsync(new IncidenciaParticipante
                {
                    IdGrupo = _grupoId,
                    IdUsuario = _turistaId,
                    IdCheckpoint = _checkpointId,
                    Tipo = TipoSeleccionado.Valor,
                    Descripcion = Descripcion.Trim(),
                    RequiereAtencion = RequiereAtencion,
                    Latitud = latitud,
                    Longitud = longitud
                }, RetirarParticipante);

                await Shell.Current.DisplayAlertAsync("Incidencia registrada", "El evento fue agregado a Operación.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al registrar incidencia: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo registrar", ObtenerMensajeError(ex), "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string ObtenerMensajeError(Exception ex)
        {
            if (ex.Message.Contains("registrar_incidencia_guia", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("PGRST202", StringComparison.OrdinalIgnoreCase))
            {
                return "Falta crear la funcion segura para incidencias en Supabase. Ejecuta el script actualizado de incidencias en el SQL Editor.";
            }

            if (ex.Message.Contains("row-level security", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("42501", StringComparison.OrdinalIgnoreCase))
            {
                return "Supabase bloqueó el registro por permisos RLS. Ejecuta el script actualizado de incidencias para habilitar el registro seguro del guía.";
            }

            return ex.Message;
        }
    }
}
