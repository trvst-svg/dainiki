namespace dainiki.Components.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string username, string password);
        Task<AuthResult> LoginAsync(string username, string password);
        Task<AuthResult> UpdatePasswordAsync(string username, string newPassword);
        Task<AuthResult> UpdatePinAsync(string username, string pin);
        Task LogoutAsync();
    }
}
