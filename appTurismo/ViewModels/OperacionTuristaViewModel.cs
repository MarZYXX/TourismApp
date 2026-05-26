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
        public ICommand EnviarSosCommand { get; }

        public OperacionTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Operación";
            CargarOperacionCommand = new Command(async () => await CargarAsync());
            AbrirRutaCommand = new Command<ViajeTurista>(async viaje => await AbrirRutaAsync(viaje));
            VerDetalleCommand = new Command<ViajeTurista>(async viaje => await VerDetalleAsync(viaje));
            EnviarSosCommand = new Command<ViajeTurista>(async viaje => await EnviarSosAsync(viaje));
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var viajes = (await _viajeService.GetTouristTripsAsync())
                    .Where(v => string.Equals(v.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
                    .Where(v => !string.Equals(v.ConfirmacionAsistencia, "No_asistira", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(v => v.FechaInicio)
                    .ToList();

                ViajesActivos.Clear();
                foreach (var viaje in viajes)
                {
                    var checkpoints = await _viajeService.GetTouristTripCheckpointsAsync(viaje.IdTourGroup);
                    viaje.TotalCheckpoints = checkpoints.Count;
                    viaje.CheckpointsCompletados = checkpoints.Count(c => c.Completado);
                    viaje.SosActivo = await _viajeService.GetActiveTouristSosAsync(viaje.IdTourGroup);
                    viaje.UltimoSosResuelto = await _viajeService.GetLatestResolvedTouristSosAsync(viaje.IdTourGroup);
                    ViajesActivos.Add(viaje);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar operación turista: {ex.Message}");
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

        private async Task EnviarSosAsync(ViajeTurista? viaje)
        {
            if (viaje == null || viaje.TieneSosActivo || IsBusy) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Enviar SOS",
                "Esta acción enviará tu ubicación actual al guía para solicitar ayuda. ¿Deseas continuar?",
                "Enviar SOS",
                "Cancelar");
            if (!confirmar) return;

            try
            {
                IsBusy = true;
                var permiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (permiso != PermissionStatus.Granted)
                {
                    permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (permiso != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Ubicación requerida",
                        "Debes permitir el acceso a tu ubicación para enviar una solicitud SOS.",
                        "OK");
                    return;
                }

                var ubicacion = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15)));
                if (ubicacion == null)
                {
                    await Shell.Current.DisplayAlertAsync("Sin ubicación", "No fue posible obtener tu ubicación actual.", "OK");
                    return;
                }

                await _viajeService.SendTouristSosAsync(viaje.IdTourGroup, ubicacion.Latitude, ubicacion.Longitude);
                await Shell.Current.DisplayAlertAsync(
                    "SOS enviado",
                    "Tu guía recibirá la alerta junto con tu ubicación actual.",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar SOS: {ex.Message}");
                var mensaje = ex.Message.Contains("registrar_sos_turista", StringComparison.OrdinalIgnoreCase)
                    ? "Falta ejecutar el script SOS en Supabase."
                    : "No fue posible enviar la solicitud SOS.";
                await Shell.Current.DisplayAlertAsync("No se pudo enviar", mensaje, "OK");
            }
            finally
            {
                IsBusy = false;
            }

            await CargarAsync();
        }
    }
}
