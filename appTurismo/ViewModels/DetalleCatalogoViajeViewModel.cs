using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class DetalleCatalogoViajeViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private ViajeCatalogo? _viaje;

        public ViajeCatalogo? Viaje
        {
            get => _viaje;
            private set
            {
                _viaje = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneViaje));
            }
        }

        public bool TieneViaje => Viaje != null;
        public ObservableCollection<Checkpoint> Checkpoints { get; } = new();
        public ICommand InscribirseCommand { get; }
        public event Action? CheckpointsCargados;

        public DetalleCatalogoViajeViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Detalle del viaje";
            InscribirseCommand = new Command(async () => await InscribirseAsync());
        }

        public async Task CargarAsync(string grupoId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId)) return;

            try
            {
                IsBusy = true;
                Viaje = await _viajeService.GetCatalogTripAsync(grupoId);
                if (Viaje == null)
                {
                    await Shell.Current.DisplayAlertAsync("Viaje no disponible", "Este recorrido ya no está disponible para inscripción.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Checkpoints.Clear();
                foreach (var punto in await _viajeService.GetCatalogTripCheckpointsAsync(grupoId))
                {
                    Checkpoints.Add(punto);
                }
                CheckpointsCargados?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar detalle del catálogo: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible cargar la información del recorrido.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InscribirseAsync()
        {
            if (Viaje == null || IsBusy) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Unirme al viaje",
                $"¿Deseas inscribirte en {Viaje.Nombre}?",
                "Unirme",
                "Cancelar");
            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.JoinAvailableTripAsync(Viaje.IdTourGroup);
                await Shell.Current.DisplayAlertAsync(
                    "Inscripción realizada",
                    "El viaje ya aparece en Mis Viajes. Ahora puedes confirmar tu asistencia.",
                    "OK");
                await Shell.Current.GoToAsync("//TuristaTabs/UserPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inscribir desde detalle: {ex.Message}");
                var mensaje = ex.Message.Contains("inscribirse_viaje_turista", StringComparison.OrdinalIgnoreCase)
                    ? "Falta ejecutar el script de catálogo en Supabase."
                    : "El viaje ya no está disponible o se quedó sin cupo.";
                await Shell.Current.DisplayAlertAsync("No fue posible inscribirte", mensaje, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
