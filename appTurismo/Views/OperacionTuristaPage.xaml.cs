using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class OperacionTuristaPage : ContentPage
{
    private readonly OperacionTuristaViewModel _viewModel;

    public OperacionTuristaPage(OperacionTuristaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CargarOperacionCommand.Execute(null);
    }
}
