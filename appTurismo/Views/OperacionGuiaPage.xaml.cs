using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class OperacionGuiaPage : ContentPage
{
    private readonly OperacionGuiaViewModel _viewModel;

    public OperacionGuiaPage(OperacionGuiaViewModel viewModel)
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
