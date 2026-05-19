using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using appTurismo.DataMapper;
using appTurismo.Models;

namespace appTurismo.Services
{
    public class SupabaseUserService : IUserService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly UserMapper _userMapper;

        public SupabaseUserService(Supabase.Client supabaseClient, UserMapper userMapper)
        {
            _supabaseClient = supabaseClient;
            _userMapper = userMapper;
        }

        public bool IsUserLoggedIn() => _supabaseClient.Auth.CurrentSession != null;

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                return session != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Login: {ex.Message}");
                return false;
            }
        }

        // Standard backward compatibility mapping method fallback 
        public Task<bool> RegisterAsync(string email, string password, Models.Supabase.User profileData)
        {
            return RegisterWithRoleAsync(email, password, profileData, "turista");
        }

        // Full customized role-aware registration tracking delivery engine
        public async Task<bool> RegisterWithRoleAsync(string email, string password, Models.Supabase.User profileData, string roleName)
        {
            try
            {
                var signUpOptions = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "nombre", profileData.Nombre },
                        { "apellido_paterno", profileData.Apellido_paterno },
                        { "apellido_materno", profileData.Apellido_materno },
                        { "telefono", profileData.Telefono },
                        { "rol_elegido", roleName } // This maps directly to what our SQL function looks for!
                    }
                };

                var session = await _supabaseClient.Auth.SignUp(email, password, signUpOptions);
                return session != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Registro con Rol: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try { await _supabaseClient.Auth.SignOut(); }
            catch (Exception ex) { Debug.WriteLine($"Error en Logout: {ex.Message}"); }
        }

        public async Task<List<Models.UserDTO>> GetUsersAsync()
        {
            try
            {
                var response = await _supabaseClient.From<Models.Supabase.User>().Get();
                var users = response.Models;
                var usersDTO = new List<Models.UserDTO>();
                foreach (var user in users)
                {
                    usersDTO.Add(await _userMapper.UserToDTO(user));
                }
                return usersDTO;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving users: {ex.Message}");
                return new List<Models.UserDTO>();
            }
        }

        public async Task CreateUserAsync(Models.Supabase.User user) =>
            await _supabaseClient.From<Models.Supabase.User>().Insert(user);

        public async Task UpdateUserAsync(Models.Supabase.User user)
        {
            await _supabaseClient
                .From<Models.Supabase.User>()
                .Where(x => x.Id_usuario == user.Id_usuario)
                .Set(x => x.Nombre, user.Nombre)
                .Set(x => x.Apellido_paterno, user.Apellido_paterno)
                .Set(x => x.Apellido_materno, user.Apellido_materno)
                .Set(x => x.Correo_electronico, user.Correo_electronico)
                .Set(x => x.Telefono, user.Telefono)
                .Update();
        }

        public async Task DeleteUserAsync(Guid id) =>
            await _supabaseClient.From<Models.Supabase.User>().Where(x => x.Id_usuario == id).Delete();
    }
}