using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class OperacionGuiaPage : ContentPage
{
    public OperacionGuiaPage(OperacionGuiaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
