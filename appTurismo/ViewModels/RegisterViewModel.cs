using appTurismo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using appTurismo.Helpers;

namespace appTurismo.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IConnectivity _connectivity;

        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _nombre = string.Empty;
        [ObservableProperty] private string _apellidoPaterno = string.Empty;
        [ObservableProperty] private string _apellidoMaterno = string.Empty;
        [ObservableProperty] private string _telefono = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TuristaButtonColor))]
        [NotifyPropertyChangedFor(nameof(GuiaButtonColor))]
        private string _selectedRole = "turista";

        public string TuristaButtonColor => SelectedRole == "turista" ? "DarkCyan" : "LightGray";
        public string GuiaButtonColor => SelectedRole == "guia" ? "DarkCyan" : "LightGray";

        public RegisterViewModel(IUserService userService, IConnectivity connectivity)
        {
            Title = "Crear Cuenta";
            _userService = userService;
            _connectivity = connectivity;
        }

        [RelayCommand]
        private void SelectRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return;

            var lowerRole = role.ToLower().Trim();
            if (lowerRole == "turista" || lowerRole == "guia")
            {
                SelectedRole = lowerRole;
            }
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(ApellidoPaterno) ||
                string.IsNullOrWhiteSpace(Telefono))
            {
                await Shell.Current.DisplayAlertAsync("Faltan datos", "Nombre, apellido paterno, teléfono, correo y contraseña son obligatorios.", "OK");
                return;
            }

            if (!FormValidators.IsValidEmail(Email))
            {
                await Shell.Current.DisplayAlertAsync("Correo inválido", "Ingresa un correo válido, por ejemplo usuario@correo.com.", "OK");
                return;
            }

            if (!FormValidators.IsValidPassword(Password))
            {
                await Shell.Current.DisplayAlertAsync("Contraseña inválida", "La contraseña debe tener al menos 6 caracteres.", "OK");
                return;
            }

            if (!FormValidators.IsValidPhone(Telefono))
            {
                await Shell.Current.DisplayAlertAsync("Teléfono inválido", "Ingresa exactamente 10 dígitos, sin espacios ni guiones.", "OK");
                return;
            }

            if (!FormValidators.IsValidName(Nombre) ||
                !FormValidators.IsValidName(ApellidoPaterno) ||
                !FormValidators.IsValidName(ApellidoMaterno, required: false))
            {
                await Shell.Current.DisplayAlertAsync("Nombre inválido", "Nombre y apellidos solo pueden incluir letras, espacios, guion o apóstrofo.", "OK");
                return;
            }

            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlertAsync("Sin conexión", "No hay acceso a internet.", "OK");
                return;
            }

            IsBusy = true;

            var nuevoUsuario = new Models.Supabase.User
            {
                Nombre = Nombre.Trim(),
                Apellido_paterno = ApellidoPaterno.Trim(),
                Apellido_materno = ApellidoMaterno.Trim(),
                Telefono = Telefono.Trim(),
                Correo_electronico = Email.Trim()
            };

            bool success = await _userService.RegisterWithRoleAsync(Email.Trim(), Password, nuevoUsuario, SelectedRole);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlertAsync("¡Bienvenido!", "Registro exitoso.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "No se pudo registrar. Verifica que el correo no esté en uso.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        
        }
    }
}
