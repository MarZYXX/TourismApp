namespace appTurismo.Services
{
    public interface IUserService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password, Models.Supabase.User profileData);

        // ADD THIS METHOD TO SUPPORT EXPLICIT ROLE PASSING:
        Task<bool> RegisterWithRoleAsync(string email, string password, Models.Supabase.User profileData, string roleName);

        Task LogoutAsync();
        bool IsUserLoggedIn();
        Task DeleteUserAsync(Guid id);
        Task<List<Models.UserDTO>> GetUsersAsync();
        Task CreateUserAsync(Models.Supabase.User user);
        Task UpdateUserAsync(Models.Supabase.User user);
    }
}