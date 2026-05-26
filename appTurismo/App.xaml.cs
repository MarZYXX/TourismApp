using Microsoft.Extensions.DependencyInjection;

namespace appTurismo
{
    public partial class App : Application
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly Services.IUserService _userService;

        public App(Supabase.Client supabaseClient, Services.IUserService userService)
        {
            InitializeComponent();
            _supabaseClient = supabaseClient;
            _userService = userService;
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

                if (_supabaseClient.Auth.CurrentSession != null)
                {
                    var userRole = await _userService.GetCurrentRoleAsync();
                    if (userRole == "guia")
                    {
                        await Shell.Current.GoToAsync("//GuiaTabs/AdminPage");
                    }
                    else if (userRole == "turista")
                    {
                        await Shell.Current.GoToAsync("//TuristaTabs/UserPage");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Supabase Startup Error]: {ex.Message}");
            }
        }
    }
}
