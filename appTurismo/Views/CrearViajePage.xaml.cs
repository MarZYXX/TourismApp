using appTurismo.ViewModels;
using appTurismo.Models;
using Mapsui.UI.Maui;
using Mapsui.Projections;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class CrearViajePage : ContentPage
{
    private CrearViajeViewModel _viewModel;
    private int _contadorPuntos = 1;

    public CrearViajePage(CrearViajeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.CheckpointEliminado += AlEliminarCheckpoint;
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

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                mapView.MyLocationEnabled = true;
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));

                if (location != null)
                {
                    mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
                    var (miX, miY) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                    mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(miX, miY));
                    mapView.Map.Navigator.ZoomTo(10);
                }
            }
        }
        catch { }

        mapView.Refresh();
    }

    private void BtnAgregarCentro_Clicked(object? sender, EventArgs e)
    {
        var centroX = mapView.Map.Navigator.Viewport.CenterX;
        var centroY = mapView.Map.Navigator.Viewport.CenterY;

        var (longitud, latitud) = SphericalMercator.ToLonLat(centroX, centroY);

        string nombrePunto;
        if (_contadorPuntos == 1)
            nombrePunto = "Inicio de Ruta";
        else
            nombrePunto = $"Punto {_contadorPuntos}";

        var nuevoPin = new Pin(mapView)
        {
            Label = nombrePunto,
            Position = new Position(latitud, longitud),
            Type = PinType.Pin,
            Color = Colors.Red
        };

        mapView.Pins.Add(nuevoPin);
        mapView.Refresh();

        _viewModel.CheckpointsNuevos.Add(new Checkpoint
        {
            Nombre = nombrePunto,
            Latitud = latitud,
            Longitud = longitud,
            Completado = false
        });

        _contadorPuntos++;

        _ = DisplayAlertAsync("Éxito", $"'{nombrePunto}' agregado a la ruta.", "OK");
    }

    private void AlEliminarCheckpoint(Checkpoint checkpoint)
    {
        var pin = mapView.Pins.FirstOrDefault(p =>
            p.Position.Latitude == checkpoint.Latitud &&
            p.Position.Longitude == checkpoint.Longitud);

        if (pin != null)
        {
            mapView.Pins.Remove(pin);
            mapView.Refresh();
        }
    }
}
