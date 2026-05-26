using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class OperacionGuiaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;

        public ObservableCollection<GrupoTour> ViajesEnCurso { get; } = new();
        public ObservableCollection<IncidenciaOperacion> IncidenciasPendientes { get; } = new();
        public ObservableCollection<IncidenciaOperacion> HistorialIncidencias { get; } = new();
        public ICommand CargarOperacionCommand { get; }
        public ICommand AbrirSeguimientoCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand VerIncidenciaCommand { get; }

        public OperacionGuiaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Operacion";

            CargarOperacionCommand = new Command(async () => await CargarOperacionAsync());
            AbrirSeguimientoCommand = new Command<GrupoTour>(async viaje => await AbrirSeguimientoAsync(viaje));
            VerDetalleCommand = new Command<GrupoTour>(async viaje => await VerDetalleAsync(viaje));
            VerIncidenciaCommand = new Command<IncidenciaOperacion>(async incidencia => await VerIncidenciaAsync(incidencia));
        }

        private async Task CargarOperacionAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var viajes = await _viajeService.GetActiveGuideTripsAsync();

                ViajesEnCurso.Clear();
                foreach (var viaje in viajes)
                {
                    ViajesEnCurso.Add(viaje);
                }

                var incidencias = await _viajeService.GetGuideIncidentsAsync();
                IncidenciasPendientes.Clear();
                HistorialIncidencias.Clear();
                foreach (var incidencia in incidencias)
                {
                    if (incidencia.EsCerrada)
                    {
                        HistorialIncidencias.Add(incidencia);
                    }
                    else
                    {
                        IncidenciasPendientes.Add(incidencia);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar operacion: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible consultar los recorridos activos.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task AbrirSeguimientoAsync(GrupoTour? viaje)
        {
            if (viaje == null) return;

            Preferences.Set("ViajeSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("MapaPage");
        }

        private static async Task VerDetalleAsync(GrupoTour? viaje)
        {
            if (viaje == null) return;

            Preferences.Set("ViajeSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("DetalleViajePage");
        }

        private static async Task VerIncidenciaAsync(IncidenciaOperacion? incidencia)
        {
            if (incidencia == null) return;

            Preferences.Set("IncidenciaSeleccionada", incidencia.Incidencia.IdIncidencia);
            await Shell.Current.GoToAsync("DetalleIncidenciaPage");
        }
    }
}
