using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class DetalleViajeViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private GrupoTour? _viaje;
        private Models.Supabase.User? _turistaSeleccionado;

        public GrupoTour? Viaje
        {
            get => _viaje;
            private set
            {
                _viaje = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneViaje));
                OnPropertyChanged(nameof(PuedeAdministrarParticipantes));
                OnPropertyChanged(nameof(PuedeIniciarRecorrido));
                OnPropertyChanged(nameof(EsRecorridoIniciado));
            }
        }

        public bool TieneViaje => Viaje != null;
        public bool PuedeAdministrarParticipantes =>
            string.Equals(Viaje?.Estado, "Plan", StringComparison.OrdinalIgnoreCase);
        public bool PuedeIniciarRecorrido => PuedeAdministrarParticipantes;
        public bool EsRecorridoIniciado =>
            string.Equals(Viaje?.Estado, "Activo", StringComparison.OrdinalIgnoreCase);

        public ObservableCollection<Models.Supabase.User> Participantes { get; } = new();
        public ObservableCollection<Models.Supabase.User> TuristasDisponibles { get; } = new();

        public Models.Supabase.User? TuristaSeleccionado
        {
            get => _turistaSeleccionado;
            set
            {
                _turistaSeleccionado = value;
                OnPropertyChanged();
            }
        }

        public ICommand VerMapaCommand { get; }
        public ICommand GestionarCheckpointsCommand { get; }
        public ICommand AgregarParticipanteCommand { get; }
        public ICommand QuitarParticipanteCommand { get; }
        public ICommand IniciarRecorridoCommand { get; }

        public DetalleViajeViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Detalle del viaje";

            VerMapaCommand = new Command(async () => await AbrirMapaAsync());
            GestionarCheckpointsCommand = new Command(async () => await AbrirCheckpointsAsync());
            AgregarParticipanteCommand = new Command(async () => await AgregarParticipanteAsync());
            QuitarParticipanteCommand = new Command<Models.Supabase.User>(async turista => await QuitarParticipanteAsync(turista));
            IniciarRecorridoCommand = new Command(async () => await IniciarRecorridoAsync());
        }

        public async Task CargarViajeAsync(string grupoId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId))
            {
                return;
            }

            try
            {
                IsBusy = true;
                Viaje = await _viajeService.GetGuideTripAsync(grupoId);

                if (Viaje == null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Viaje no disponible",
                        "No se encontró este recorrido dentro de tus viajes.",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                await CargarParticipantesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar viaje: {ex.Message}");
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "No fue posible cargar el detalle del viaje.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AbrirMapaAsync()
        {
            if (Viaje == null) return;

            Preferences.Set("ViajeSeleccionado", Viaje.IdTourGroup);
            await Shell.Current.GoToAsync("MapaPage");
        }

        private async Task AbrirCheckpointsAsync()
        {
            if (Viaje == null) return;

            Preferences.Set("ViajeSeleccionado", Viaje.IdTourGroup);
            await Shell.Current.GoToAsync("GestionarCheckpointsPage");
        }

        private async Task CargarParticipantesAsync()
        {
            if (Viaje == null) return;

            Participantes.Clear();
            var participantes = await _viajeService.GetAssignedTouristsAsync(Viaje.IdTourGroup);
            foreach (var turista in participantes)
            {
                Participantes.Add(turista);
            }

            TuristasDisponibles.Clear();
            if (PuedeAdministrarParticipantes)
            {
                var disponibles = await _viajeService.GetAvailableTouristsAsync(Viaje.IdTourGroup);
                foreach (var turista in disponibles)
                {
                    TuristasDisponibles.Add(turista);
                }
            }

            TuristaSeleccionado = null;
        }

        private async Task AgregarParticipanteAsync()
        {
            if (Viaje == null || TuristaSeleccionado == null || !PuedeAdministrarParticipantes)
            {
                await Shell.Current.DisplayAlertAsync("Selecciona un turista", "Elige un turista disponible para agregarlo.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                await _viajeService.AddParticipantAsync(Viaje.IdTourGroup, TuristaSeleccionado.Id_usuario);
                await CargarParticipantesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al agregar participante: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo agregar", ObtenerMensajeAsignacion(ex), "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string ObtenerMensajeAsignacion(Exception ex)
        {
            var mensaje = ex.Message;

            if (mensaje.Contains("row-level security", StringComparison.OrdinalIgnoreCase) ||
                mensaje.Contains("permission denied", StringComparison.OrdinalIgnoreCase))
            {
                return "Supabase rechazo la operacion por permisos RLS. Es necesario autorizar al guia para asignar participantes en sus viajes.";
            }

            if (mensaje.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                mensaje.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return "Este turista ya esta asignado al recorrido.";
            }

            if (mensaje.Contains("cupo", StringComparison.OrdinalIgnoreCase))
            {
                return mensaje;
            }

            return $"Supabase respondio: {mensaje}";
        }

        private async Task QuitarParticipanteAsync(Models.Supabase.User? turista)
        {
            if (Viaje == null || turista == null || !PuedeAdministrarParticipantes) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Retirar participante",
                $"Deseas quitar a {turista.Nombre} {turista.Apellido_paterno} del viaje?",
                "Quitar",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.RemoveParticipantAsync(Viaje.IdTourGroup, turista.Id_usuario);
                await CargarParticipantesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al quitar participante: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo quitar", "No fue posible actualizar la lista de participantes.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task IniciarRecorridoAsync()
        {
            if (Viaje == null || !PuedeIniciarRecorrido) return;

            var mensaje = Participantes.Count == 0
                ? "Este viaje no tiene turistas asignados. Deseas iniciarlo de todas formas para realizar pruebas?"
                : $"Deseas iniciar el recorrido con {Participantes.Count} participante(s)?";

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Iniciar recorrido",
                mensaje,
                "Iniciar",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.StartTripAsync(Viaje.IdTourGroup);
                Preferences.Set("ViajeSeleccionado", Viaje.IdTourGroup);
                await Shell.Current.GoToAsync("//GuiaTabs/OperacionGuiaPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al iniciar recorrido: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo iniciar", "No fue posible cambiar el viaje a estado Activo.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
