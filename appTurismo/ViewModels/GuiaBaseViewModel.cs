using System.Windows.Input;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public abstract class GuiaBaseViewModel : BaseViewModel
    {
        private readonly IUserService _userService;

        public ICommand CerrarSesionCommand { get; }

        protected GuiaBaseViewModel(IUserService userService)
        {
            _userService = userService;
            CerrarSesionCommand = new Command(async () => await CerrarSesionAsync());
        }

        private async Task CerrarSesionAsync()
        {
            var confirmar = await Shell.Current.DisplayAlertAsync(
                "Cerrar sesión",
                "¿Deseas cerrar tu sesión?",
                "Salir",
                "Cancelar");

            if (!confirmar)
            {
                return;
            }

            await _userService.LogoutAsync();
            Preferences.Default.Remove("ViajeSeleccionado");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
