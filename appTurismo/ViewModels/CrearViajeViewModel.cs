using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class CrearViajeViewModel : BaseViewModel
    {
        private readonly IViajeService _viajeService;

        private string _nombrePaquete = string.Empty;
        public string NombrePaquete
        {
            get => _nombrePaquete;
            set { _nombrePaquete = value; OnPropertyChanged(); }
        }

        private string _descripcion = string.Empty;
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(); }
        }

        private string _puntoEncuentro = string.Empty;
        public string PuntoEncuentro
        {
            get => _puntoEncuentro;
            set { _puntoEncuentro = value; OnPropertyChanged(); }
        }

        private string _cupoMaximo = string.Empty;
        public string CupoMaximo
        {
            get => _cupoMaximo;
            set { _cupoMaximo = value; OnPropertyChanged(); }
        }

        private DateTime _fechaInicio = DateTime.Today.AddDays(1);
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { _fechaInicio = value; OnPropertyChanged(); }
        }

        private TimeSpan _horaInicio = new(9, 0, 0);
        public TimeSpan HoraInicio
        {
            get => _horaInicio;
            set { _horaInicio = value; OnPropertyChanged(); }
        }

        public DateTime FechaMinima => DateTime.Today;

        private bool _isTracingRoute;
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

        public ObservableCollection<Checkpoint> CheckpointsNuevos { get; set; }
        public ICommand GuardarViajeCommand { get; }
        public ICommand EliminarCheckpointCommand { get; }
        public ICommand AbrirTrazadorCommand { get; }
        public ICommand CerrarTrazadorCommand { get; }
        public event Action<Checkpoint>? CheckpointEliminado;

        public CrearViajeViewModel(IViajeService viajeService)
        {
            _viajeService = viajeService;
            Title = "Nuevo Viaje";
            CheckpointsNuevos = new ObservableCollection<Checkpoint>();
            GuardarViajeCommand = new Command(async () => await GuardarViaje());
            EliminarCheckpointCommand = new Command<Checkpoint>(EliminarCheckpoint);
            AbrirTrazadorCommand = new Command(() => IsTracingRoute = true);
            CerrarTrazadorCommand = new Command(() => IsTracingRoute = false);
        }

        private async Task GuardarViaje()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(NombrePaquete))
            {
                await Shell.Current.DisplayAlertAsync("Falta el nombre", "Escribe un nombre para el viaje.", "OK");
                return;
            }

            if (CheckpointsNuevos.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Falta la ruta", "Agrega al menos un checkpoint antes de guardar.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PuntoEncuentro))
            {
                await Shell.Current.DisplayAlertAsync("Falta el encuentro", "Escribe el punto de encuentro del recorrido.", "OK");
                return;
            }

            if (!int.TryParse(CupoMaximo, out var cupo) || cupo <= 0)
            {
                await Shell.Current.DisplayAlertAsync("Cupo inválido", "Indica un cupo máximo mayor que cero.", "OK");
                return;
            }

            var fechaProgramada = FechaInicio.Date.Add(HoraInicio);
            if (fechaProgramada <= DateTime.Now)
            {
                await Shell.Current.DisplayAlertAsync("Fecha inválida", "Programa el viaje para una fecha y hora futuras.", "OK");
                return;
            }

            if (CheckpointsNuevos.Any(cp => string.IsNullOrWhiteSpace(cp.Nombre)))
            {
                await Shell.Current.DisplayAlertAsync("Checkpoint sin nombre", "Asigna un nombre a todos los puntos de la ruta.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var nuevoViaje = new GrupoTour
                {
                    IdTourGroup = Guid.NewGuid().ToString(),
                    Nombre = NombrePaquete,
                    Descripcion = Descripcion,
                    PuntoEncuentro = PuntoEncuentro,
                    CupoMaximo = cupo,
                    FechaInicio = fechaProgramada
                };

                await _viajeService.CreateTripAsync(nuevoViaje, CheckpointsNuevos.ToList());
                await Shell.Current.DisplayAlertAsync("Viaje creado", "La ruta se guardó correctamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar viaje: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error al guardar", "No se pudo crear la ruta. Revisa tu sesión o la configuración de Supabase.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EliminarCheckpoint(Checkpoint? checkpoint)
        {
            if (checkpoint == null || !CheckpointsNuevos.Remove(checkpoint)) return;

            CheckpointEliminado?.Invoke(checkpoint);
        }
    }
}
