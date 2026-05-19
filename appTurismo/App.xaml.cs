using Microsoft.Extensions.DependencyInjection;

namespace appTurismo
{
    public partial class App : Application
    {
        private readonly Supabase.Client _supabaseClient;

        public App(Supabase.Client supabaseClient)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                // Safely handle initialization in a non-blocking background routine
                await _supabaseClient.InitializeAsync();

                // If our CustomSupabaseSessionHandler successfully loaded a cached valid token
                if (_supabaseClient.Auth.CurrentSession != null)
                {
                    // Leapfrog the login view directly into the MainPage structure
                    await Shell.Current.GoToAsync("//MainPage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Supabase Startup Error]: {ex.Message}");
            }
        }
    }
}