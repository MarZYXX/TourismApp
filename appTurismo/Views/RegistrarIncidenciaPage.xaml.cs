using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class RegistrarIncidenciaPage : ContentPage
{
    private readonly RegistrarIncidenciaViewModel _viewModel;

    public RegistrarIncidenciaPage(RegistrarIncidenciaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Preparar(
            Preferences.Get("ViajeSeleccionado", string.Empty),
            Preferences.Get("CheckpointSeleccionado", string.Empty),
            Preferences.Get("CheckpointNombre", "Checkpoint"),
            Preferences.Get("IncidenciaTuristaId", string.Empty),
            Preferences.Get("IncidenciaTuristaNombre", "Turista"));
    }
}
