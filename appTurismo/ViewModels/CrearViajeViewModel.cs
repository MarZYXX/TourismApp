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

        private string _nombrePaquete;
        public string NombrePaquete
        {
            get => _nombrePaquete;
            set { _nombrePaquete = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Checkpoint> CheckpointsNuevos { get; set; }
        public ICommand GuardarViajeCommand { get; }

        public CrearViajeViewModel(IViajeService viajeService)
        {
            _viajeService = viajeService;
            Title = "Nuevo Viaje";
            CheckpointsNuevos = new ObservableCollection<Checkpoint>();
            GuardarViajeCommand = new Command(async () => await GuardarViaje());
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

            IsBusy = true;
            try
            {
                var nuevoViaje = new GrupoTour
                {
                    IdTourGroup = Guid.NewGuid().ToString(),
                    Nombre = NombrePaquete,
                    FechaInicio = DateTime.Now
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
    }
}
