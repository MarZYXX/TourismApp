using appTurismo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace appTurismo.Services
{
    public interface IUserService
    {
        bool IsUserLoggedIn();

        // ¡ESTA ES LA LÍNEA QUE CAMBIAMOS A string?!
        Task<string?> LoginAsync(string email, string password);

        Task<bool> RegisterAsync(string email, string password, Models.Supabase.User profileData);
        Task<bool> RegisterWithRoleAsync(string email, string password, Models.Supabase.User profileData, string roleName);
        Task<List<UserDTO>> GetUsersAsync();
        Task CreateUserAsync(Models.Supabase.User user);
        Task UpdateUserAsync(Models.Supabase.User user);
        Task DeleteUserAsync(Guid id);
        Task LogoutAsync();
    }
}