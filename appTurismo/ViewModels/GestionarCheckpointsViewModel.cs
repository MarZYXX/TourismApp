using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services; // Asumimos que tienes un servicio o lo leeremos directo de Supabase

namespace appTurismo.ViewModels
{
    public class GestionarCheckpointsViewModel : BaseViewModel
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IViajeService _viajeService;
        private string _grupoId = string.Empty;
        private bool _esRecorridoActivo;

        public ObservableCollection<Checkpoint> ListaCheckpoints { get; set; }

        public ICommand MarcarCompletadoCommand { get; }
        public ICommand AbrirAsistenciaCommand { get; }
        public ICommand FinalizarRecorridoCommand { get; }
        public event System.Action? CheckpointsActualizados;

        public bool EsRecorridoActivo
        {
            get => _esRecorridoActivo;
            private set
            {
                _esRecorridoActivo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PuedeFinalizarRecorrido));
            }
        }

        public bool PuedeFinalizarRecorrido =>
            EsRecorridoActivo && ListaCheckpoints.Count > 0 && ListaCheckpoints.All(cp => cp.Completado);

        public GestionarCheckpointsViewModel(Supabase.Client supabaseClient, IViajeService viajeService)
        {
            _supabaseClient = supabaseClient;
            _viajeService = viajeService;
            Title = "Control de Checkpoints";
            ListaCheckpoints = new ObservableCollection<Checkpoint>();

            // Comando para cambiar el estado del checkpoint en Supabase
            MarcarCompletadoCommand = new Command<Checkpoint>(async (cp) => await CompletarCheckpoint(cp));
            AbrirAsistenciaCommand = new Command<Checkpoint>(async cp => await AbrirAsistenciaAsync(cp));
            FinalizarRecorridoCommand = new Command(async () => await FinalizarRecorridoAsync());
        }

        // Esta función se llamará cuando la pantalla se abra para cargar los puntos de ese viaje específico
        public async Task CargarCheckpoints(string grupoId)
        {
            if (string.IsNullOrEmpty(grupoId)) return;
            _grupoId = grupoId;
            IsBusy = true;

            try
            {
                var viaje = await _viajeService.GetGuideTripAsync(_grupoId);
                EsRecorridoActivo = string.Equals(viaje?.Estado, "Activo", System.StringComparison.OrdinalIgnoreCase);

                // Vamos a la nube y traemos solo los puntos de ESTE viaje ordenados por número
                var respuesta = await _supabaseClient.From<Checkpoint>()
                                                     .Where(c => c.IdGrupo == _grupoId)
                                                     .Order(c => c.Orden, Supabase.Postgrest.Constants.Ordering.Ascending)
                                                     .Get();

                ListaCheckpoints.Clear();
                foreach (var checkpoint in respuesta.Models)
                {
                    ListaCheckpoints.Add(checkpoint);
                }

                OnPropertyChanged(nameof(PuedeFinalizarRecorrido));
                CheckpointsActualizados?.Invoke();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar checkpoints: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CompletarCheckpoint(Checkpoint cp)
        {
            if (cp == null || cp.Completado) return;

            if (!EsRecorridoActivo)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Recorrido no iniciado",
                    "Inicia el recorrido desde el detalle para registrar avances de checkpoints.",
                    "OK");
                return;
            }

            cp.Completado = true; // Lo marcamos a nivel local para que la UI cambie de color

            try
            {
                // Le decimos a Supabase que actualice solo este punto
                await _supabaseClient.From<Checkpoint>()
                                     .Where(x => x.IdCheckpoint == cp.IdCheckpoint)
                                     .Set(x => x.Completado, true)
                                     .Update();

                // Refrescamos la lista para que el cambio se note en pantalla
                await CargarCheckpoints(_grupoId);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar: {ex.Message}");
                cp.Completado = false; // Revertimos si hay error
            }
        }

        private async Task AbrirAsistenciaAsync(Checkpoint? checkpoint)
        {
            if (checkpoint == null) return;

            if (!EsRecorridoActivo)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Recorrido no iniciado",
                    "La asistencia solo se registra durante un recorrido activo.",
                    "OK");
                return;
            }

            Preferences.Set("CheckpointSeleccionado", checkpoint.IdCheckpoint);
            Preferences.Set("CheckpointNombre", checkpoint.Nombre);
            await Shell.Current.GoToAsync("AsistenciaCheckpointPage");
        }

        private async Task FinalizarRecorridoAsync()
        {
            if (!PuedeFinalizarRecorrido) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Finalizar recorrido",
                "Todos los checkpoints estan completos. Deseas cerrar este recorrido?",
                "Finalizar",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.CompleteTripAsync(_grupoId);
                await Shell.Current.DisplayAlertAsync("Recorrido completado", "El viaje fue marcado como Completado.", "OK");
                await Shell.Current.GoToAsync("//GuiaTabs/OperacionGuiaPage");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al finalizar recorrido: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo finalizar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
