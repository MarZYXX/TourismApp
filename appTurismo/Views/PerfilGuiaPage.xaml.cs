using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class PerfilGuiaPage : ContentPage
{
    public PerfilGuiaPage(PerfilGuiaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
