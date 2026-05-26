using appTurismo.ViewModels;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class MapaPage : ContentPage
{
    private readonly GestionarCheckpointsViewModel _viewModel;

    public MapaPage(GestionarCheckpointsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.CheckpointsActualizados += ActualizarPines;
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

        var grupoId = Preferences.Get("ViajeSeleccionado", string.Empty);
        if (!string.IsNullOrWhiteSpace(grupoId))
        {
            await _viewModel.CargarCheckpoints(grupoId);
        }

        var (x, y) = SphericalMercator.FromLonLat(-99.1332, 19.4326);
        mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
        mapView.Refresh();

        await ActivarGPSAsync();
    }

    private void ActualizarPines()
    {
        mapView.Pins.Clear();

        foreach (var punto in _viewModel.ListaCheckpoints)
        {
            mapView.Pins.Add(new Pin(mapView)
            {
                Label = punto.Nombre,
                Position = new Position(punto.Latitud, punto.Longitud),
                Type = PinType.Pin,
                Color = punto.Completado ? Colors.Green : Colors.Red
            });
        }

        mapView.Refresh();
    }

    private async Task ActivarGPSAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                return;
            }

            mapView.MyLocationEnabled = true;
            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

            if (location == null)
            {
                return;
            }

            mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
            var (miX, miY) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(miX, miY));
            mapView.Map.Navigator.ZoomTo(10);
            mapView.Refresh();
        }
        catch (Exception)
        {
            // The route remains usable when location access is unavailable.
        }
    }
}
