using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using appTurismo.Models;
using Supabase.Postgrest.Exceptions;

namespace appTurismo.Services
{
    public class SupabaseUserService : IUserService
    {
        private readonly Supabase.Client _supabaseClient;
        public SupabaseUserService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public bool IsUserLoggedIn() => _supabaseClient.Auth.CurrentSession != null;

        public async Task<string?> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                if (session?.User == null) return null;

                return await GetCurrentRoleAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Login RPC: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetCurrentRoleAsync()
        {
            var userId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (!Guid.TryParse(userId, out var userUuid))
            {
                return null;
            }

            var rol = await _supabaseClient.Rpc<string>("get_user_role", new Dictionary<string, object>
            {
                { "user_uuid", userUuid }
            });

            return rol?.ToLower().Trim();
        }

        public async Task<Models.Supabase.User?> GetCurrentUserProfileAsync()
        {
            var userId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (!Guid.TryParse(userId, out var userUuid))
            {
                return null;
            }

            var respuesta = await _supabaseClient.From<Models.Supabase.User>()
                                                .Where(u => u.Id_usuario == userUuid)
                                                .Get();
            return respuesta.Models.FirstOrDefault();
        }

        public async Task UpdateCurrentUserProfileAsync(
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            string telefono)
        {
            var userId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (!Guid.TryParse(userId, out var userUuid))
            {
                throw new InvalidOperationException("No hay un usuario autenticado.");
            }

            await _supabaseClient.From<Models.Supabase.User>()
                                 .Where(u => u.Id_usuario == userUuid)
                                 .Set(u => u.Nombre, nombre)
                                 .Set(u => u.Apellido_paterno, apellidoPaterno)
                                 .Set(u => u.Apellido_materno, apellidoMaterno)
                                 .Set(u => u.Telefono, telefono)
                                 .Update();
        }

        public async Task<bool> RegisterAsync(string email, string password, Models.Supabase.User profileData)
        {
            return await RegisterWithRoleAsync(email, password, profileData, "turista");
        }

        public async Task<bool> RegisterWithRoleAsync(string email, string password, Models.Supabase.User profileData, string roleName)
        {
            try
            {
                // 1. Obtener ID del Rol vía RPC (esto es muy eficiente)
                var resolvedRoleIdStr = await _supabaseClient.Rpc<string>("get_role_id_by_name", new Dictionary<string, object>
        {
            { "role_name", roleName.ToLower().Trim() }
        });

                if (string.IsNullOrEmpty(resolvedRoleIdStr)) return false;
                Guid resolvedRoleId = Guid.Parse(resolvedRoleIdStr);

                // 2. Empaquetar datos en metadatos (Esto es lo que hace que tu Trigger funcione)
                var options = new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
            {
                { "nombre", profileData.Nombre },
                { "apellido_paterno", profileData.Apellido_paterno },
                { "apellido_materno", profileData.Apellido_materno },
                { "telefono", profileData.Telefono },
                { "id_rol", resolvedRoleId }
            }
                };

                // 3. Registro. ¡YA NO HAGAS .Insert() MANUAL! 
                // El trigger en la base de datos creará la fila por ti.
                var response = await _supabaseClient.Auth.SignUp(email, password, options);

                return response?.User != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR REGISTRO]: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserDTO>> GetUsersAsync() => new List<UserDTO>();

        public async Task CreateUserAsync(Models.Supabase.User user) =>
            await _supabaseClient.From<Models.Supabase.User>().Insert(user);

        public async Task UpdateUserAsync(Models.Supabase.User user) =>
            await _supabaseClient.From<Models.Supabase.User>().Where(x => x.Id_usuario == user.Id_usuario).Update(user);

        public async Task DeleteUserAsync(Guid id) =>
            await _supabaseClient.From<Models.Supabase.User>().Where(x => x.Id_usuario == id).Delete();

        public async Task LogoutAsync() => await _supabaseClient.Auth.SignOut();
    }
}
