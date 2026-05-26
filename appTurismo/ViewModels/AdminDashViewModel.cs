using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class AdminDashViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private readonly List<GrupoTour> _todosViajes = new();
        private string _filtroSeleccionado = "Todos";

        public ObservableCollection<GrupoTour> ViajesFiltrados { get; } = new();

        public string FiltroSeleccionado
        {
            get => _filtroSeleccionado;
            private set
            {
                _filtroSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloFiltro));
            }
        }

        public string TituloFiltro => FiltroSeleccionado switch
        {
            "Plan" => "Viajes planificados",
            "Activo" => "Viajes en curso",
            "Completado" => "Viajes finalizados",
            "Cancelado" => "Viajes cancelados",
            _ => "Todos mis viajes"
        };

        public int TotalViajes => _todosViajes.Count;
        public int TotalPlanificados => _todosViajes.Count(v => EsEstado(v, "Plan"));
        public int TotalEnCurso => _todosViajes.Count(v => EsEstado(v, "Activo"));
        public int TotalFinalizados => _todosViajes.Count(v => EsEstado(v, "Completado"));
        public int TotalCancelados => _todosViajes.Count(v => EsEstado(v, "Cancelado"));

        public ICommand CargarDashboardCommand { get; }
        public ICommand IrCrearViajeCommand { get; }
        public ICommand FiltrarViajesCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand VerMapaCommand { get; }
        public ICommand EditarViajeCommand { get; }
        public ICommand CancelarViajeCommand { get; }
        public ICommand ReactivarViajeCommand { get; }

        public AdminDashViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Mis Viajes";

            CargarDashboardCommand = new Command(async () => await CargarDashboardAsync());
            IrCrearViajeCommand = new Command(async () => await Shell.Current.GoToAsync("CrearViajePage"));
            FiltrarViajesCommand = new Command<string>(AplicarFiltro);

            VerDetalleCommand = new Command<GrupoTour>(async viaje =>
            {
                if (viaje == null) return;

                Preferences.Set("ViajeSeleccionado", viaje.IdTourGroup);
                await Shell.Current.GoToAsync("DetalleViajePage");
            });

            VerMapaCommand = new Command<GrupoTour>(async viaje =>
            {
                if (viaje == null) return;

                Preferences.Set("ViajeSeleccionado", viaje.IdTourGroup);
                await Shell.Current.GoToAsync("MapaPage");
            });

            EditarViajeCommand = new Command<GrupoTour>(async viaje =>
            {
                if (viaje == null || !viaje.EsPlanificado) return;

                Preferences.Set("ViajeSeleccionado", viaje.IdTourGroup);
                await Shell.Current.GoToAsync("EditarViajePage");
            });

            CancelarViajeCommand = new Command<GrupoTour>(async viaje => await CancelarViajeAsync(viaje));
            ReactivarViajeCommand = new Command<GrupoTour>(async viaje => await ReactivarViajeAsync(viaje));
        }

        private async Task CargarDashboardAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var viajes = await _viajeService.GetGuideTripsAsync();

                _todosViajes.Clear();
                _todosViajes.AddRange(viajes.OrderByDescending(v => v.FechaInicio));
                NotificarContadores();
                AplicarFiltro(FiltroSeleccionado);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AplicarFiltro(string? filtro)
        {
            FiltroSeleccionado = string.IsNullOrWhiteSpace(filtro) ? "Todos" : filtro;

            var viajes = FiltroSeleccionado == "Todos"
                ? _todosViajes
                : _todosViajes.Where(v => EsEstado(v, FiltroSeleccionado));

            ViajesFiltrados.Clear();
            foreach (var viaje in viajes)
            {
                ViajesFiltrados.Add(viaje);
            }
        }

        private void NotificarContadores()
        {
            OnPropertyChanged(nameof(TotalViajes));
            OnPropertyChanged(nameof(TotalPlanificados));
            OnPropertyChanged(nameof(TotalEnCurso));
            OnPropertyChanged(nameof(TotalFinalizados));
            OnPropertyChanged(nameof(TotalCancelados));
        }

        private static bool EsEstado(GrupoTour viaje, string estado) =>
            string.Equals(viaje.Estado, estado, StringComparison.OrdinalIgnoreCase);

        private async Task CancelarViajeAsync(GrupoTour? viaje)
        {
            if (viaje == null || !viaje.EsPlanificado) return;

            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Cancelar viaje",
                "El viaje dejara de estar disponible, pero se conservara para poder reactivarlo.",
                "Cancelar viaje",
                "Volver");
            if (!confirmar) return;

            try
            {
                IsBusy = true;
                await _viajeService.CancelTripAsync(viaje.IdTourGroup);
                IsBusy = false;
                await CargarDashboardAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cancelar viaje: {ex.Message}");
                await Shell.Current.DisplayAlertAsync(
                    "No se pudo cancelar",
                    "Ejecuta el ajuste SQL para habilitar el estado Cancelado en Supabase.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReactivarViajeAsync(GrupoTour? viaje)
        {
            if (viaje == null || !viaje.EsCancelado) return;

            try
            {
                IsBusy = true;
                await _viajeService.ReactivateTripAsync(viaje.IdTourGroup);
                IsBusy = false;
                await CargarDashboardAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reactivar viaje: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo reactivar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
