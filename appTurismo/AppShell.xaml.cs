namespace appTurismo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("CrearViajePage", typeof(Views.CrearViajePage));
            Routing.RegisterRoute("DetalleViajePage", typeof(Views.DetalleViajePage));
            Routing.RegisterRoute("MapaPage", typeof(Views.MapaPage));
            Routing.RegisterRoute("GestionarCheckpointsPage", typeof(Views.GestionarCheckpointsPage));
        }
    }
}
