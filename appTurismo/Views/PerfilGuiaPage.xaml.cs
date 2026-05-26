using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class PerfilGuiaPage : ContentPage
{
    private readonly PerfilGuiaViewModel _viewModel;

    public PerfilGuiaPage(PerfilGuiaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CargarPerfilCommand.Execute(null);
    }
}
