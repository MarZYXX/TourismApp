using System.Collections.ObjectModel;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class MapaViajeTuristaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private ViajeTurista? _viaje;

        public ViajeTurista? Viaje
        {
            get => _viaje;
            private set { _viaje = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Checkpoint> Checkpoints { get; } = new();
        public event Action? CheckpointsCargados;

        public MapaViajeTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Ruta del viaje";
        }

        public async Task CargarAsync(string grupoId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId)) return;

            try
            {
                IsBusy = true;
                Viaje = await _viajeService.GetTouristTripAsync(grupoId);
                Checkpoints.Clear();
                foreach (var punto in await _viajeService.GetTouristTripCheckpointsAsync(grupoId))
                {
                    Checkpoints.Add(punto);
                }
                CheckpointsCargados?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar mapa turista: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible cargar la ruta.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
