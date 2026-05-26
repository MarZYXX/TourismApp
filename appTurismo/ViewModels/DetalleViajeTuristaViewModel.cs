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
            }
        }

        public bool TieneViaje => Viaje != null;
        public ObservableCollection<Checkpoint> Checkpoints { get; } = new();
        public ICommand AbrirRutaCompletaCommand { get; }
        public event Action? CheckpointsCargados;

        public DetalleViajeTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Detalle del viaje";
            AbrirRutaCompletaCommand = new Command(async () => await AbrirRutaCompletaAsync());
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
                    await Shell.Current.DisplayAlertAsync("Viaje no disponible", "Este recorrido no esta asignado a tu cuenta.", "OK");
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
    }
}
