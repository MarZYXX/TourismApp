using appTurismo.ViewModels;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class DetalleCatalogoViajePage : ContentPage
{
    private readonly DetalleCatalogoViajeViewModel _viewModel;

    public DetalleCatalogoViajePage(DetalleCatalogoViajeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.CheckpointsCargados += MostrarVistaPrevia;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PrepararMapa();
        await _viewModel.CargarAsync(Preferences.Get("ViajeCatalogoSeleccionado", string.Empty));
    }

    private void PrepararMapa()
    {
        mapPreview.Map ??= new Mapsui.Map();
        mapPreview.Map.Layers.Clear();
        mapPreview.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
    }

    private void MostrarVistaPrevia()
    {
        mapPreview.Pins.Clear();
        foreach (var punto in _viewModel.Checkpoints)
        {
            mapPreview.Pins.Add(new Pin(mapPreview)
            {
                Label = punto.Nombre,
                Position = new Position(punto.Latitud, punto.Longitud),
                Type = PinType.Pin,
                Color = Colors.Red
            });
        }

        var primero = _viewModel.Checkpoints.FirstOrDefault();
        if (primero != null)
        {
            var (x, y) = SphericalMercator.FromLonLat(primero.Longitud, primero.Latitud);
            mapPreview.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
            mapPreview.Map.Navigator.ZoomTo(8);
        }

        mapPreview.Refresh();
    }
}
