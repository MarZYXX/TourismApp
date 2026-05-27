namespace appTurismo.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(ViewModels.RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void TogglePassword_Clicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Source = PasswordEntry.IsPassword ? "eye.svg" : "eye_off.svg";
        SemanticProperties.SetDescription(
            TogglePasswordButton,
            PasswordEntry.IsPassword ? "Mostrar contraseña" : "Ocultar contraseña");
    }
}
