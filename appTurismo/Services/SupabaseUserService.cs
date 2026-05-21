using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using appTurismo.Models;

namespace appTurismo.Services
{
    public class SupabaseUserService : IUserService
    {
        private readonly Supabase.Client _supabaseClient;

        // Quitamos _userMapper porque nos estaba causando errores al no estar declarado
        public SupabaseUserService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public bool IsUserLoggedIn() => _supabaseClient.Auth.CurrentSession != null;

        public async Task<string?> LoginAsync(string email, string password)
        {
            try
            {
                // 1. Autenticación estándar
                var session = await _supabaseClient.Auth.SignIn(email, password);
                if (session?.User == null) return null;

                // 2. CONSULTA DIRECTA (Sin usar RPC)
                // Buscamos el usuario en la tabla 'usuarios'
                var userResponse = await _supabaseClient.From<Models.Supabase.User>()
                    .Where(u => u.Id_usuario == Guid.Parse(session.User.Id))
                    .Get();

                var usuario = userResponse.Models.FirstOrDefault();
                if (usuario == null) return null;

                // 3. Buscamos el rol directamente en la tabla 'roles'
                var roleResponse = await _supabaseClient.From<Models.Supabase.Role>()
                    .Where(r => r.Id_rol == usuario.Id_rol)
                    .Get();

                var rol = roleResponse.Models.FirstOrDefault();

                // Retornamos el nombre del rol (ej: "guia" o "turista")
                return rol?.Nombre?.ToLower().Trim();
            }
            catch (Exception ex)
            {
                // Esto imprimirá el error real en la ventana de "Salida"
                Debug.WriteLine($"[ERROR FATAL DE SUPABASE]: {ex.Message}");
                Debug.WriteLine($"[STACK TRACE]: {ex.StackTrace}");
                return null;
            }
        }

        // --- ESTE MÉTODO SOLUCIONA EL ERROR CS0535 ---
        public async Task<bool> RegisterAsync(string email, string password, Models.Supabase.User profileData)
        {
            return await RegisterWithRoleAsync(email, password, profileData, "turista");
        }

        public async Task<bool> RegisterWithRoleAsync(string email, string password, Models.Supabase.User profileData, string roleName)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignUp(email, password);
                if (response?.User == null) return false;

                var roleResponse = await _supabaseClient.From<Models.Supabase.Role>()
                    .Where(r => r.Nombre == roleName.ToLower().Trim())
                    .Get();

                var rol = roleResponse.Models.FirstOrDefault();
                if (rol == null) return false;

                profileData.Id_usuario = Guid.Parse(response.User.Id);
                profileData.Correo_electronico = email;
                profileData.Id_rol = rol.Id_rol;
                profileData.Created_at = DateTime.UtcNow;

                await _supabaseClient.From<Models.Supabase.User>().Insert(profileData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR DE REGISTRO DETALLADO]: {ex.Message}");
                Debug.WriteLine($"[TIPO DE ERROR]: {ex.GetType().Name}");
                return false;
            }
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            // Nota: Si necesitas GetUsersAsync, tendrías que inyectar el UserMapper de nuevo.
            // Por ahora, para evitar el error CS0103, devolvemos una lista vacía.
            Debug.WriteLine("GetUsersAsync no implementado actualmente.");
            return new List<UserDTO>();
        }

        public async Task CreateUserAsync(Models.Supabase.User user) =>
            await _supabaseClient.From<Models.Supabase.User>().Insert(user);

        public async Task UpdateUserAsync(Models.Supabase.User user)
        {
            await _supabaseClient
                .From<Models.Supabase.User>()
                .Where(x => x.Id_usuario == user.Id_usuario)
                .Update(user);
        }

        public async Task DeleteUserAsync(Guid id) =>
            await _supabaseClient.From<Models.Supabase.User>().Where(x => x.Id_usuario == id).Delete();

        public async Task LogoutAsync()
        {
            await _supabaseClient.Auth.SignOut();
        }
    }
}