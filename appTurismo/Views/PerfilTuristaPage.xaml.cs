using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class PerfilTuristaPage : ContentPage
{
    private readonly PerfilTuristaViewModel _viewModel;

    public PerfilTuristaPage(PerfilTuristaViewModel viewModel)
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
