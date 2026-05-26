using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class PerfilGuiaViewModel : GuiaBaseViewModel
    {
        public PerfilGuiaViewModel(IUserService userService) : base(userService)
        {
            Title = "Perfil";
        }
    }
}
