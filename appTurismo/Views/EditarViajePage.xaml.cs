using appTurismo.Models;
using appTurismo.ViewModels;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class EditarViajePage : ContentPage
{
    private readonly EditarViajeViewModel _viewModel;

    public EditarViajePage(EditarViajeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.CheckpointsActualizados += MostrarPines;
        _viewModel.TrazadorAbierto += CentrarTrazador;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (mapView.Map == null)
        {
            mapView.Map = new Mapsui.Map();
        }

        mapView.Map.Layers.Clear();
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        await _viewModel.CargarAsync(Preferences.Get("ViajeSeleccionado", string.Empty));
        MostrarPines();

        if (_viewModel.Checkpoints.Count == 0)
        {
            await CentrarEnGpsAsync();
        }
    }

    private void AplicarPuntoMapa_Clicked(object? sender, EventArgs e)
    {
        var centroX = mapView.Map.Navigator.Viewport.CenterX;
        var centroY = mapView.Map.Navigator.Viewport.CenterY;
        var (longitud, latitud) = SphericalMercator.ToLonLat(centroX, centroY);
        _viewModel.AplicarPuntoDelMapa(latitud, longitud);
        MostrarPines();
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
                Color = Colors.Red
            });
        }

        mapView.Refresh();
    }

    private void CentrarTrazador(Checkpoint? checkpoint)
    {
        MostrarPines();

        if (checkpoint == null)
        {
            if (_viewModel.Checkpoints.Count > 0)
            {
                CentrarEn(_viewModel.Checkpoints[0]);
            }
            return;
        }

        CentrarEn(checkpoint);
    }

    private void CentrarEn(Checkpoint checkpoint)
    {
        var (x, y) = SphericalMercator.FromLonLat(checkpoint.Longitud, checkpoint.Latitud);
        mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
        mapView.Map.Navigator.ZoomTo(10);
        mapView.Refresh();
    }

    private async Task CentrarEnGpsAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location == null) return;

            var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
            mapView.Map.Navigator.ZoomTo(10);
            mapView.Refresh();
        }
        catch
        {
            // El trazador sigue disponible si no hay ubicacion.
        }
    }
}
