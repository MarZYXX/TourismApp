using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class CrearViajePage : ContentPage
{
    public CrearViajePage(CrearViajeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}