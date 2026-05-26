using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class AsistenciaCheckpointPage : ContentPage
{
    private readonly AsistenciaCheckpointViewModel _viewModel;

    public AsistenciaCheckpointPage(AsistenciaCheckpointViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var grupoId = Preferences.Get("ViajeSeleccionado", string.Empty);
        var checkpointId = Preferences.Get("CheckpointSeleccionado", string.Empty);
        var checkpointNombre = Preferences.Get("CheckpointNombre", "Checkpoint");
        await _viewModel.CargarAsync(grupoId, checkpointId, checkpointNombre);
    }
}
