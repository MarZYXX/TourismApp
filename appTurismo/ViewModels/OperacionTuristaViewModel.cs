using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class OperacionTuristaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;

        public ObservableCollection<ViajeTurista> ViajesActivos { get; } = new();
        public ICommand CargarOperacionCommand { get; }
        public ICommand AbrirRutaCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public OperacionTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Operacion";
            CargarOperacionCommand = new Command(async () => await CargarAsync());
            AbrirRutaCommand = new Command<ViajeTurista>(async viaje => await AbrirRutaAsync(viaje));
            VerDetalleCommand = new Command<ViajeTurista>(async viaje => await VerDetalleAsync(viaje));
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var viajes = (await _viajeService.GetTouristTripsAsync())
                    .Where(v => string.Equals(v.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(v => v.FechaInicio)
                    .ToList();

                ViajesActivos.Clear();
                foreach (var viaje in viajes)
                {
                    var checkpoints = await _viajeService.GetTouristTripCheckpointsAsync(viaje.IdTourGroup);
                    viaje.TotalCheckpoints = checkpoints.Count;
                    viaje.CheckpointsCompletados = checkpoints.Count(c => c.Completado);
                    ViajesActivos.Add(viaje);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar operacion turista: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible consultar tu recorrido activo.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task AbrirRutaAsync(ViajeTurista? viaje)
        {
            if (viaje == null) return;
            Preferences.Set("ViajeTuristaSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("MapaViajeTuristaPage");
        }

        private static async Task VerDetalleAsync(ViajeTurista? viaje)
        {
            if (viaje == null) return;
            Preferences.Set("ViajeTuristaSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("DetalleViajeTuristaPage");
        }
    }
}
