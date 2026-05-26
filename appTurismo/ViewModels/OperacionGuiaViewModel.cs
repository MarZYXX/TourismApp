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
        public ObservableCollection<SosOperacion> AlertasSos { get; } = new();
        public ObservableCollection<SosOperacion> HistorialSos { get; } = new();
        public ObservableCollection<IncidenciaOperacion> IncidenciasPendientes { get; } = new();
        public ObservableCollection<IncidenciaOperacion> HistorialIncidencias { get; } = new();
        public ICommand CargarOperacionCommand { get; }
        public ICommand AbrirSeguimientoCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand VerIncidenciaCommand { get; }
        public ICommand VerUbicacionSosCommand { get; }
        public ICommand ResolverSosCommand { get; }

        public OperacionGuiaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Operación";

            CargarOperacionCommand = new Command(async () => await CargarOperacionAsync());
            AbrirSeguimientoCommand = new Command<GrupoTour>(async viaje => await AbrirSeguimientoAsync(viaje));
            VerDetalleCommand = new Command<GrupoTour>(async viaje => await VerDetalleAsync(viaje));
            VerIncidenciaCommand = new Command<IncidenciaOperacion>(async incidencia => await VerIncidenciaAsync(incidencia));
            VerUbicacionSosCommand = new Command<SosOperacion>(async sos => await VerUbicacionSosAsync(sos));
            ResolverSosCommand = new Command<SosOperacion>(async sos => await ResolverSosAsync(sos));
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

                AlertasSos.Clear();
                foreach (var sos in await _viajeService.GetActiveGuideSosAsync())
                {
                    AlertasSos.Add(sos);
                }
                HistorialSos.Clear();
                foreach (var sos in await _viajeService.GetResolvedGuideSosAsync())
                {
                    HistorialSos.Add(sos);
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
                System.Diagnostics.Debug.WriteLine($"Error al cargar operación: {ex.Message}");
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

        private static async Task VerUbicacionSosAsync(SosOperacion? sos)
        {
            if (sos == null) return;

            Preferences.Set("SosLatitud", sos.Solicitud.Latitud);
            Preferences.Set("SosLongitud", sos.Solicitud.Longitud);
            Preferences.Set("SosTurista", sos.NombreTurista);
            Preferences.Set("SosViaje", sos.NombreViaje);
            await Shell.Current.GoToAsync("MapaSosPage");
        }

        private async Task ResolverSosAsync(SosOperacion? sos)
        {
            if (sos == null || IsBusy) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Resolver SOS",
                $"Confirmas que la solicitud de {sos.NombreTurista} ya fue atendida?",
                "Resolver",
                "Cancelar");
            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.ResolveGuideSosAsync(sos.Solicitud.IdSos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al resolver SOS: {ex.Message}");
                var mensaje = ex.Message.Contains("resolver_sos_guia", StringComparison.OrdinalIgnoreCase)
                    ? "Falta ejecutar el script SOS en Supabase."
                    : "No fue posible resolver la alerta SOS.";
                await Shell.Current.DisplayAlertAsync("No se pudo resolver", mensaje, "OK");
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await CargarOperacionAsync();
        }
    }
}
