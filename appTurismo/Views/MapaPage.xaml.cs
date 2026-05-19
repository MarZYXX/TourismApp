using Mapsui.UI.Maui;
using Position = Mapsui.UI.Maui.Position; // Para evitar confusiones con otras librerías

namespace appTurismo.Views;

public partial class MapaPage : ContentPage
{
    public MapaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. Limpiamos los pines por si la pantalla se abre dos veces
        mapView.Pins.Clear();

        // 2. Creamos los 3 pines simulando los checkpoints de Supabase
        var pin1 = new Pin(mapView)
        {
            Label = "Punto 1: Estacionamiento",
            Position = new Position(19.4326, -99.1332), // Las coordenadas que pusimos en ViajeService
            Type = PinType.Pin,
            Color = Colors.Red
        };

        var pin2 = new Pin(mapView)
        {
            Label = "Punto 2: Cabaña",
            Position = new Position(19.4330, -99.1340),
            Type = PinType.Pin,
            Color = Colors.Orange
        };

        var pin3 = new Pin(mapView)
        {
            Label = "Punto 3: Cascada",
            Position = new Position(19.4340, -99.1350),
            Type = PinType.Pin,
            Color = Colors.Green
        };

        // 3. Los agregamos al mapa
        mapView.Pins.Add(pin1);
        mapView.Pins.Add(pin2);
        mapView.Pins.Add(pin3);

        // 4. Convertimos la latitud y longitud, y creamos el MPoint que el mapa exige
        var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(-99.1340, 19.4330);
        var centroDelMapa = new Mapsui.MPoint(x, y);

        // Movemos la cámara del mapa
        mapView.Map.Navigator.CenterOn(centroDelMapa);
    }
}