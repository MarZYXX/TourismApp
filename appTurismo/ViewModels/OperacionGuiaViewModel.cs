using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class OperacionGuiaViewModel : GuiaBaseViewModel
    {
        public OperacionGuiaViewModel(IUserService userService) : base(userService)
        {
            Title = "Operación";
        }
    }
}
