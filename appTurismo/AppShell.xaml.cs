namespace appTurismo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("CrearViajePage", typeof(Views.CrearViajePage));
            Routing.RegisterRoute("EditarViajePage", typeof(Views.EditarViajePage));
            Routing.RegisterRoute("DetalleViajePage", typeof(Views.DetalleViajePage));
            Routing.RegisterRoute("MapaPage", typeof(Views.MapaPage));
            Routing.RegisterRoute("GestionarCheckpointsPage", typeof(Views.GestionarCheckpointsPage));
            Routing.RegisterRoute("AsistenciaCheckpointPage", typeof(Views.AsistenciaCheckpointPage));
            Routing.RegisterRoute("RegistrarIncidenciaPage", typeof(Views.RegistrarIncidenciaPage));
            Routing.RegisterRoute("DetalleIncidenciaPage", typeof(Views.DetalleIncidenciaPage));
            Routing.RegisterRoute("DetalleViajeTuristaPage", typeof(Views.DetalleViajeTuristaPage));
            Routing.RegisterRoute("MapaViajeTuristaPage", typeof(Views.MapaViajeTuristaPage));
        }
    }
}
