using appTurismo.ViewModels;

namespace appTurismo.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel vm)
        {
            if (vm.GetUsersCommand.CanExecute(null))
            {
                vm.GetUsersCommand.Execute(null);
            }
        }
    }
}
