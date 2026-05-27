using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class GestionarCheckpointsPage : ContentPage
{
    private readonly GestionarCheckpointsViewModel _viewModel;

    public GestionarCheckpointsPage(GestionarCheckpointsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var idViajeActual = Preferences.Get("ViajeSeleccionado", "");
        if (!string.IsNullOrEmpty(idViajeActual))
        {
            await _viewModel.CargarCheckpoints(idViajeActual);
        }
    }
}
