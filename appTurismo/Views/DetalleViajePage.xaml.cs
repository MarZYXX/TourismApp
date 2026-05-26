using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class DetalleViajePage : ContentPage
{
    private readonly DetalleViajeViewModel _viewModel;

    public DetalleViajePage(DetalleViajeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var grupoId = Preferences.Get("ViajeSeleccionado", string.Empty);
        await _viewModel.CargarViajeAsync(grupoId);
    }
}
