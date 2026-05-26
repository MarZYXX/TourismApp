using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class CatalogoViajesViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;

        public ObservableCollection<ViajeCatalogo> ViajesDisponibles { get; } = new();
        public ICommand CargarCommand { get; }
        public ICommand InscribirseCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public CatalogoViajesViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Explorar";
            CargarCommand = new Command(async () => await CargarAsync());
            InscribirseCommand = new Command<ViajeCatalogo>(async viaje => await InscribirseAsync(viaje));
            VerDetalleCommand = new Command<ViajeCatalogo>(async viaje => await VerDetalleAsync(viaje));
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ViajesDisponibles.Clear();
                foreach (var viaje in await _viajeService.GetAvailableCatalogTripsAsync())
                {
                    ViajesDisponibles.Add(viaje);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar catálogo de viajes: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Catálogo no disponible", "No fue posible consultar los viajes disponibles.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task VerDetalleAsync(ViajeCatalogo? viaje)
        {
            if (viaje == null) return;
            Preferences.Set("ViajeCatalogoSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("DetalleCatalogoViajePage");
        }

        private async Task InscribirseAsync(ViajeCatalogo? viaje)
        {
            if (viaje == null || IsBusy) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Unirme al viaje",
                $"¿Deseas inscribirte en {viaje.Nombre}?",
                "Unirme",
                "Cancelar");
            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.JoinAvailableTripAsync(viaje.IdTourGroup);
                ViajesDisponibles.Remove(viaje);
                await Shell.Current.DisplayAlertAsync(
                    "Inscripción realizada",
                    "El viaje se agregó a Mis Viajes. Desde ahí podrás consultar la ruta y confirmar asistencia.",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inscribir turista: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No fue posible inscribirte", ResolverMensaje(ex), "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string ResolverMensaje(Exception ex)
        {
            if (ex.Message.Contains("inscribirse_viaje_turista", StringComparison.OrdinalIgnoreCase))
            {
                return "Falta ejecutar el script de catálogo en Supabase antes de utilizar esta función.";
            }

            if (ex.Message.Contains("cupo", StringComparison.OrdinalIgnoreCase))
            {
                return "El viaje ya no tiene lugares disponibles.";
            }

            return "El viaje pudo haber cambiado o ya no está disponible. Actualiza el catálogo e intenta de nuevo.";
        }
    }
}
