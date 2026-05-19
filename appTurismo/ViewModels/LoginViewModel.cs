using appTurismo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace appTurismo.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IConnectivity _connectivity;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        public LoginViewModel(IUserService userService, IConnectivity connectivity)
        {
            Title = "Iniciar Sesión";
            _userService = userService;
            _connectivity = connectivity;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Campos Vacíos", "Por favor ingresa tu correo y contraseña.", "OK");
                return;
            }

            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Sin Conexión", "Revisa tu conexión a internet.", "OK");
                return;
            }

            IsBusy = true;
            bool success = await _userService.LoginAsync(Email, Password);
            IsBusy = false;

            if (success)
            {
                // Route safely into your main container shell mapping context
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Credenciales incorrectas. Intenta de nuevo.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }
    }
}