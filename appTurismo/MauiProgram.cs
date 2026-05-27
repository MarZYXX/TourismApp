using appTurismo.Services;
using appTurismo.ViewModels;
using appTurismo.DataMapper;
using appTurismo.Views;
using Microsoft.Extensions.Logging;

using SkiaSharp.Views.Maui.Controls.Hosting;

namespace appTurismo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            string supabaseUrl = "https://afymvarqqnromdikgwha.supabase.co";
            string supabaseKey = "sb_publishable_ZVc1rTyPdKZWAGqTbu52DQ_TAmgGbHF";

            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);

            builder.Services.AddSingleton(supabaseClient);

            builder.Services.AddSingleton<Supabase.Client>(supabaseClient);
            builder.Services.AddSingleton<IUserService, SupabaseUserService>();
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IStorageService, SupabaseStorageService>();
            builder.Services.AddSingleton<UserMapper>();

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<RegisterPage>();

            builder.Services.AddSingleton<IViajeService, ViajeService>();
            builder.Services.AddTransient<AdminDashViewModel>();
            builder.Services.AddTransient<AdminPage>();
            builder.Services.AddTransient<OperacionGuiaViewModel>();
            builder.Services.AddTransient<OperacionGuiaPage>();
            builder.Services.AddTransient<PerfilGuiaViewModel>();
            builder.Services.AddTransient<PerfilGuiaPage>();

            builder.Services.AddTransient<CrearViajeViewModel>();
            builder.Services.AddTransient<CrearViajePage>();
            builder.Services.AddTransient<EditarViajeViewModel>();
            builder.Services.AddTransient<EditarViajePage>();
            builder.Services.AddTransient<DetalleViajeViewModel>();
            builder.Services.AddTransient<DetalleViajePage>();
            builder.Services.AddTransient<MapaPage>();

            builder.Services.AddTransient<GestionarCheckpointsViewModel>();
            builder.Services.AddTransient<GestionarCheckpointsPage>();
            builder.Services.AddTransient<AsistenciaCheckpointViewModel>();
            builder.Services.AddTransient<AsistenciaCheckpointPage>();
            builder.Services.AddTransient<RegistrarIncidenciaViewModel>();
            builder.Services.AddTransient<RegistrarIncidenciaPage>();
            builder.Services.AddTransient<DetalleIncidenciaViewModel>();
            builder.Services.AddTransient<DetalleIncidenciaPage>();

            builder.Services.AddTransient<ViajesTuristaViewModel>();
            builder.Services.AddTransient<UserPage>();
            builder.Services.AddTransient<CatalogoViajesViewModel>();
            builder.Services.AddTransient<CatalogoViajesPage>();
            builder.Services.AddTransient<DetalleCatalogoViajeViewModel>();
            builder.Services.AddTransient<DetalleCatalogoViajePage>();
            builder.Services.AddTransient<DetalleViajeTuristaViewModel>();
            builder.Services.AddTransient<DetalleViajeTuristaPage>();
            builder.Services.AddTransient<MapaViajeTuristaViewModel>();
            builder.Services.AddTransient<MapaViajeTuristaPage>();
            builder.Services.AddTransient<OperacionTuristaViewModel>();
            builder.Services.AddTransient<OperacionTuristaPage>();
            builder.Services.AddTransient<PerfilTuristaViewModel>();
            builder.Services.AddTransient<PerfilTuristaPage>();
            builder.Services.AddTransient<MapaSosPage>();

            return builder.Build();
        }
    }
}
