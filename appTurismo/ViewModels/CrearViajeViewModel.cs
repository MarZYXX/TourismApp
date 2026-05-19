using System;
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

        public ICommand GuardarViajeCommand { get; }

        public CrearViajeViewModel(IViajeService viajeService)
        {
            _viajeService = viajeService;
            Title = "Nuevo Viaje";
            GuardarViajeCommand = new Command(async () => await GuardarViaje());
        }

        private async Task GuardarViaje()
        {
            if (string.IsNullOrWhiteSpace(NombrePaquete)) return;

            var nuevoViaje = new GrupoTour
            {
                IdTourGroup = NombrePaquete, // Usamos el nombre que escribas como ID
                FechaInicio = DateTime.Now
            };

            await _viajeService.CreateTripAsync(nuevoViaje);

            // Regresamos a la pantalla principal
            await Shell.Current.GoToAsync("..");
        }
    }
}