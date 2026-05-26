using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class CatalogoViajesPage : ContentPage
{
    private readonly CatalogoViajesViewModel _viewModel;

    public CatalogoViajesPage(CatalogoViajesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CargarCommand.Execute(null);
    }
}
