using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class EditarViajeViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private GrupoTour? _viaje;
        private Checkpoint? _checkpointParaMover;
        private string _nombre = string.Empty;
        private string _descripcion = string.Empty;
        private string _puntoEncuentro = string.Empty;
        private string _cupoMaximo = string.Empty;
        private DateTime _fechaInicio = DateTime.Today.AddDays(1);
        private TimeSpan _horaInicio = new(9, 0, 0);
        private bool _isTracingRoute;

        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }
        public string PuntoEncuentro { get => _puntoEncuentro; set { _puntoEncuentro = value; OnPropertyChanged(); } }
        public string CupoMaximo { get => _cupoMaximo; set { _cupoMaximo = value; OnPropertyChanged(); } }
        public DateTime FechaInicio { get => _fechaInicio; set { _fechaInicio = value; OnPropertyChanged(); } }
        public TimeSpan HoraInicio { get => _horaInicio; set { _horaInicio = value; OnPropertyChanged(); } }
        public DateTime FechaMinima => DateTime.Today;

        public bool IsTracingRoute
        {
            get => _isTracingRoute;
            private set
            {
                _isTracingRoute = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingDetails));
            }
        }

        public bool IsEditingDetails => !IsTracingRoute;
        public bool TieneViaje => _viaje != null;
        public bool MoviendoCheckpoint => _checkpointParaMover != null;
        public string AccionMapaTexto => MoviendoCheckpoint ? "Mover checkpoint aqui" : "Agregar checkpoint aqui";
        public string TituloTrazador => MoviendoCheckpoint ? $"Mover: {_checkpointParaMover?.Nombre}" : "Editar ruta";

        public ObservableCollection<Checkpoint> Checkpoints { get; } = new();

        public ICommand GuardarCambiosCommand { get; }
        public ICommand AbrirTrazadorCommand { get; }
        public ICommand CerrarTrazadorCommand { get; }
        public ICommand MoverCheckpointCommand { get; }
        public ICommand EliminarCheckpointCommand { get; }
        public ICommand SubirCheckpointCommand { get; }
        public ICommand BajarCheckpointCommand { get; }
        public event Action? CheckpointsActualizados;
        public event Action<Checkpoint?>? TrazadorAbierto;

        public EditarViajeViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Editar viaje";
            GuardarCambiosCommand = new Command(async () => await GuardarAsync());
            AbrirTrazadorCommand = new Command(() => AbrirTrazador(null));
            CerrarTrazadorCommand = new Command(CerrarTrazador);
            MoverCheckpointCommand = new Command<Checkpoint>(AbrirTrazador);
            EliminarCheckpointCommand = new Command<Checkpoint>(EliminarCheckpoint);
            SubirCheckpointCommand = new Command<Checkpoint>(checkpoint => CambiarOrden(checkpoint, -1));
            BajarCheckpointCommand = new Command<Checkpoint>(checkpoint => CambiarOrden(checkpoint, 1));
        }

        public async Task CargarAsync(string grupoId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId)) return;

            try
            {
                IsBusy = true;
                _viaje = await _viajeService.GetGuideTripAsync(grupoId);
                if (_viaje == null || !_viaje.EsPlanificado)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Edicion no disponible",
                        "Solo los viajes planificados pueden editarse.",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Nombre = _viaje.Nombre;
                Descripcion = _viaje.Descripcion ?? string.Empty;
                PuntoEncuentro = _viaje.PuntoEncuentro ?? string.Empty;
                CupoMaximo = _viaje.CupoMaximo?.ToString() ?? string.Empty;
                FechaInicio = _viaje.FechaInicio.Date;
                HoraInicio = _viaje.FechaInicio.TimeOfDay;

                Checkpoints.Clear();
                foreach (var checkpoint in await _viajeService.GetPlannedTripCheckpointsAsync(grupoId))
                {
                    Checkpoints.Add(checkpoint);
                }

                OnPropertyChanged(nameof(TieneViaje));
                CheckpointsActualizados?.Invoke();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void AplicarPuntoDelMapa(double latitud, double longitud)
        {
            if (_checkpointParaMover != null)
            {
                _checkpointParaMover.Latitud = latitud;
                _checkpointParaMover.Longitud = longitud;
                _checkpointParaMover = null;
                NotificarModoMapa();
                CheckpointsActualizados?.Invoke();
                return;
            }

            Checkpoints.Add(new Checkpoint
            {
                IdCheckpoint = string.Empty,
                IdGrupo = _viaje?.IdTourGroup ?? string.Empty,
                Nombre = Checkpoints.Count == 0 ? "Inicio de Ruta" : $"Punto {Checkpoints.Count + 1}",
                Latitud = latitud,
                Longitud = longitud,
                Completado = false,
                Orden = Checkpoints.Count + 1
            });
            CheckpointsActualizados?.Invoke();
        }

        private void AbrirTrazador(Checkpoint? checkpoint)
        {
            _checkpointParaMover = checkpoint;
            NotificarModoMapa();
            IsTracingRoute = true;
            TrazadorAbierto?.Invoke(checkpoint);
        }

        private void CerrarTrazador()
        {
            _checkpointParaMover = null;
            NotificarModoMapa();
            IsTracingRoute = false;
        }

        private void EliminarCheckpoint(Checkpoint? checkpoint)
        {
            if (checkpoint == null) return;
            Checkpoints.Remove(checkpoint);
            CheckpointsActualizados?.Invoke();
        }

        private void CambiarOrden(Checkpoint? checkpoint, int desplazamiento)
        {
            if (checkpoint == null) return;

            var indice = Checkpoints.IndexOf(checkpoint);
            var destino = indice + desplazamiento;
            if (indice < 0 || destino < 0 || destino >= Checkpoints.Count) return;

            Checkpoints.Move(indice, destino);
            CheckpointsActualizados?.Invoke();
        }

        private void NotificarModoMapa()
        {
            OnPropertyChanged(nameof(MoviendoCheckpoint));
            OnPropertyChanged(nameof(AccionMapaTexto));
            OnPropertyChanged(nameof(TituloTrazador));
        }

        private async Task GuardarAsync()
        {
            if (_viaje == null || IsBusy) return;

            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(PuntoEncuentro))
            {
                await Shell.Current.DisplayAlertAsync("Datos incompletos", "Indica el nombre y punto de encuentro.", "OK");
                return;
            }

            if (!int.TryParse(CupoMaximo, out var cupo) || cupo <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Cupo inválido", "Indica un cupo mayor que cero.", "OK");
                return;
            }

            if (Checkpoints.Count == 0 || Checkpoints.Any(p => string.IsNullOrWhiteSpace(p.Nombre)))
            {
                await Shell.Current.DisplayAlertAsync("Ruta incompleta", "Conserva al menos un checkpoint y asigna nombre a todos los puntos.", "OK");
                return;
            }

            var fechaProgramada = FechaInicio.Date.Add(HoraInicio);
            if (fechaProgramada <= DateTime.Now)
            {
                await Shell.Current.DisplayAlertAsync("Fecha inválida", "Programa el viaje para una fecha y hora futuras.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                _viaje.Nombre = Nombre.Trim();
                _viaje.Descripcion = Descripcion.Trim();
                _viaje.PuntoEncuentro = PuntoEncuentro.Trim();
                _viaje.CupoMaximo = cupo;
                _viaje.FechaInicio = fechaProgramada;
                await _viajeService.UpdatePlannedTripAsync(_viaje, Checkpoints.ToList());
                await Shell.Current.DisplayAlertAsync("Viaje actualizado", "La informacion y la ruta fueron modificadas correctamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al editar viaje: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo actualizar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
