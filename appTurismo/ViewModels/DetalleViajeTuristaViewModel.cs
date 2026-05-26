using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class DetalleViajeTuristaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private ViajeTurista? _viaje;

        public ViajeTurista? Viaje
        {
            get => _viaje;
            private set
            {
                _viaje = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneViaje));
                OnPropertyChanged(nameof(PuedeConfirmarAsistencia));
            }
        }

        public bool TieneViaje => Viaje != null;
        public bool PuedeConfirmarAsistencia =>
            (string.Equals(Viaje?.Estado, "Plan", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(Viaje?.Estado, "Activo", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(Viaje?.EstadoParticipacion, "Activo", StringComparison.OrdinalIgnoreCase);
        public ObservableCollection<Checkpoint> Checkpoints { get; } = new();
        public ICommand AbrirRutaCompletaCommand { get; }
        public ICommand ConfirmarAsistenciaCommand { get; }
        public ICommand RechazarAsistenciaCommand { get; }
        public event Action? CheckpointsCargados;

        public DetalleViajeTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Detalle del viaje";
            AbrirRutaCompletaCommand = new Command(async () => await AbrirRutaCompletaAsync());
            ConfirmarAsistenciaCommand = new Command(async () => await ActualizarConfirmacionAsync("Confirmado"));
            RechazarAsistenciaCommand = new Command(async () => await ActualizarConfirmacionAsync("No_asistira"));
        }

        public async Task CargarAsync(string grupoId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId)) return;

            try
            {
                IsBusy = true;
                Viaje = await _viajeService.GetTouristTripAsync(grupoId);
                if (Viaje == null)
                {
                    await Shell.Current.DisplayAlertAsync("Viaje no disponible", "Este recorrido no está asignado a tu cuenta.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Checkpoints.Clear();
                foreach (var checkpoint in await _viajeService.GetTouristTripCheckpointsAsync(grupoId))
                {
                    Checkpoints.Add(checkpoint);
                }
                CheckpointsCargados?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar detalle turista: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible cargar el detalle del recorrido.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AbrirRutaCompletaAsync()
        {
            if (Viaje == null) return;
            Preferences.Set("ViajeTuristaSeleccionado", Viaje.IdTourGroup);
            await Shell.Current.GoToAsync("MapaViajeTuristaPage");
        }

        private async Task ActualizarConfirmacionAsync(string confirmacion)
        {
            if (Viaje == null || !PuedeConfirmarAsistencia || IsBusy) return;

            if (confirmacion == "No_asistira")
            {
                var continuar = await Shell.Current.DisplayAlertAsync(
                    "No asistire",
                    "¿Deseas informar al guía que no asistirás a este viaje?",
                    "Confirmar",
                    "Cancelar");
                if (!continuar) return;
            }

            try
            {
                IsBusy = true;
                await _viajeService.SetTouristAttendanceConfirmationAsync(Viaje.IdTourGroup, confirmacion);
                Viaje = await _viajeService.GetTouristTripAsync(Viaje.IdTourGroup);
                await Shell.Current.DisplayAlertAsync(
                    "Confirmacion registrada",
                    confirmacion == "Confirmado"
                        ? "Tu asistencia fue confirmada."
                        : "El guía verá que no asistirás.",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al confirmar asistencia: {ex.Message}");
                var mensaje = ex.Message.Contains("confirmar_asistencia_turista", StringComparison.OrdinalIgnoreCase)
                    ? "Falta ejecutar el script de confirmación de asistencia en Supabase."
                    : "No fue posible actualizar tu confirmación de asistencia.";
                await Shell.Current.DisplayAlertAsync("No se pudo registrar", mensaje, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
