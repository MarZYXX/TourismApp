using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class DetalleIncidenciaPage : ContentPage
{
    private readonly DetalleIncidenciaViewModel _viewModel;

    public DetalleIncidenciaPage(DetalleIncidenciaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync(Preferences.Get("IncidenciaSeleccionada", string.Empty));
    }
}
