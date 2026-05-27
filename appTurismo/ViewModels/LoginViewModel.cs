using appTurismo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using appTurismo.Helpers;

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
                await Shell.Current.DisplayAlertAsync("Campos Vacíos", "Por favor ingresa tu correo y contraseña.", "OK");
                return;
            }

            if (!FormValidators.IsValidEmail(Email))
            {
                await Shell.Current.DisplayAlertAsync("Correo inválido", "Ingresa un correo válido, por ejemplo usuario@correo.com.", "OK");
                return;
            }

            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlertAsync("Sin Conexión", "Revisa tu conexión a internet.", "OK");
                return;
            }

            IsBusy = true;

            string? userRole = await _userService.LoginAsync(Email.Trim(), Password);

            IsBusy = false;

            if (!string.IsNullOrEmpty(userRole))
            {
                if (userRole == "guia")
                {
                    await Shell.Current.GoToAsync("//GuiaTabs/AdminPage");
                }
                else if (userRole == "turista")
                {
                    await Shell.Current.GoToAsync("//TuristaTabs/CatalogoViajesPage");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", "Tu cuenta no tiene un rol válido asignado.", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Credenciales incorrectas o problemas de cuenta. Intenta de nuevo.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }
    }
}
