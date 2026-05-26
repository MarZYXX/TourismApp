using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class ViajesTuristaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private readonly List<ViajeTurista> _todosViajes = new();
        private string _filtro = "Todos";

        public ObservableCollection<ViajeTurista> ViajesFiltrados { get; } = new();

        public string Filtro
        {
            get => _filtro;
            private set
            {
                _filtro = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloFiltro));
            }
        }

        public string TituloFiltro => Filtro switch
        {
            "Plan" => "Próximos viajes",
            "Activo" => "Viajes en curso",
            "Completado" => "Viajes finalizados",
            "Cancelado" => "Viajes cancelados",
            _ => "Mis viajes asignados"
        };

        public int TotalProximos => _todosViajes.Count(v => EsEstado(v, "Plan"));
        public int TotalEnCurso => _todosViajes.Count(v => EsEstado(v, "Activo"));
        public int TotalFinalizados => _todosViajes.Count(v => EsEstado(v, "Completado"));

        public ICommand CargarViajesCommand { get; }
        public ICommand FiltrarCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public ViajesTuristaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Mis Viajes";
            CargarViajesCommand = new Command(async () => await CargarAsync());
            FiltrarCommand = new Command<string>(AplicarFiltro);
            VerDetalleCommand = new Command<ViajeTurista>(async viaje => await VerDetalleAsync(viaje));
        }

        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                _todosViajes.Clear();
                _todosViajes.AddRange(await _viajeService.GetTouristTripsAsync());
                OnPropertyChanged(nameof(TotalProximos));
                OnPropertyChanged(nameof(TotalEnCurso));
                OnPropertyChanged(nameof(TotalFinalizados));
                AplicarFiltro(Filtro);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar viajes del turista: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible consultar tus viajes asignados.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AplicarFiltro(string? filtro)
        {
            Filtro = string.IsNullOrWhiteSpace(filtro) ? "Todos" : filtro;
            var lista = Filtro == "Todos"
                ? _todosViajes
                : _todosViajes.Where(v => EsEstado(v, Filtro));

            ViajesFiltrados.Clear();
            foreach (var viaje in lista)
            {
                ViajesFiltrados.Add(viaje);
            }
        }

        private static async Task VerDetalleAsync(ViajeTurista? viaje)
        {
            if (viaje == null) return;
            Preferences.Set("ViajeTuristaSeleccionado", viaje.IdTourGroup);
            await Shell.Current.GoToAsync("DetalleViajeTuristaPage");
        }

        private static bool EsEstado(ViajeTurista viaje, string estado) =>
            string.Equals(viaje.Estado, estado, StringComparison.OrdinalIgnoreCase);
    }
}
