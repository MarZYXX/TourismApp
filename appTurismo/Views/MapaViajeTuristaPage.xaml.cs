using appTurismo.ViewModels;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class MapaViajeTuristaPage : ContentPage
{
    private readonly MapaViajeTuristaViewModel _viewModel;

    public MapaViajeTuristaPage(MapaViajeTuristaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.CheckpointsCargados += MostrarPines;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        mapView.Map ??= new Mapsui.Map();
        mapView.Map.Layers.Clear();
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        await _viewModel.CargarAsync(Preferences.Get("ViajeTuristaSeleccionado", string.Empty));
    }

    private void MostrarPines()
    {
        mapView.Pins.Clear();
        foreach (var punto in _viewModel.Checkpoints)
        {
            mapView.Pins.Add(new Pin(mapView)
            {
                Label = punto.Nombre,
                Position = new Position(punto.Latitud, punto.Longitud),
                Type = PinType.Pin,
                Color = punto.Completado ? Colors.Green : Colors.Red
            });
        }

        var primero = _viewModel.Checkpoints.FirstOrDefault();
        if (primero != null)
        {
            var (x, y) = SphericalMercator.FromLonLat(primero.Longitud, primero.Latitud);
            mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
            mapView.Map.Navigator.ZoomTo(8);
        }
        mapView.Refresh();
    }
}
