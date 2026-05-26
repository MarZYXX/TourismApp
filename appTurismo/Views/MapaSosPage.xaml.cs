using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.Maui.Devices.Sensors;

namespace appTurismo.Views;

public partial class MapaSosPage : ContentPage
{
    public MapaSosPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var latitud = Preferences.Get("SosLatitud", 0d);
        var longitud = Preferences.Get("SosLongitud", 0d);
        var turista = Preferences.Get("SosTurista", "Turista");
        var viaje = Preferences.Get("SosViaje", "Recorrido");

        mapView.Map ??= new Mapsui.Map();
        mapView.Map.Layers.Clear();
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        mapView.Pins.Clear();
        mapView.Pins.Add(new Pin(mapView)
        {
            Label = $"SOS - {turista}",
            Position = new Position(latitud, longitud),
            Type = PinType.Pin,
            Color = Colors.Red
        });

        var (x, y) = SphericalMercator.FromLonLat(longitud, latitud);
        mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
        mapView.Map.Navigator.ZoomTo(12);
        mapView.Refresh();

        turistaLabel.Text = $"Turista: {turista}";
        viajeLabel.Text = $"Viaje: {viaje}";
        coordenadasLabel.Text = $"Coordenadas: {latitud:F5}, {longitud:F5}";
    }
}
