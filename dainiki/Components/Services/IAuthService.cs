namespace dainiki.Components.Services
{
    public interface IAuthService
    {
        Task<AuthResult> Register(string username, string password);
        Task<AuthResult> Login(string username, string password);
        Task<AuthResult> UpdatePassword(string username, string newPassword);
        Task<AuthResult> UpdatePin(string username, string pin);
        Task Logout();
    }
}
