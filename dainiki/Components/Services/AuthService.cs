namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;

    public class AuthService : IAuthService
    {
        private readonly DainikiDbContext _context;
        private readonly ISettings _settings;

        public AuthService(DainikiDbContext context, ISettings settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            string normalized = string.Empty;
            if (username != null)
            {
                normalized = username.Trim();
            }

            if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult(false, "Username and password are required.");
            }

            bool exists = await _context.Users.AnyAsync(user => user.username == normalized);
            if (exists)
            {
                return new AuthResult(false, "That username is already registered.");
            }

            Users user = new Users
            {
                username = normalized,
                password_hash = PasswordHasher.HashPassword(password),
                auto_lock = true,
                hide_preview = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResult(true, string.Empty);
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            string normalized = string.Empty;
            if (username != null)
            {
                normalized = username.Trim();
            }

            Users? user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == normalized);
            if (user == null)
            {
                return new AuthResult(false, "Invalid username or password.");
            }

            bool matchesPassword = PasswordHasher.VerifyPassword(password, user.password_hash);
            bool matchesPin = false;
            if (!string.IsNullOrWhiteSpace(user.pin_hash))
            {
                matchesPin = PasswordHasher.VerifyPassword(password, user.pin_hash);
            }

            if (!matchesPassword && !matchesPin)
            {
                return new AuthResult(false, "Invalid username or password.");
            }

            _settings.SetUser(user);
            return new AuthResult(true, string.Empty);
        }

        public async Task<AuthResult> UpdatePasswordAsync(string username, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return new AuthResult(false, "Password cannot be empty.");
            }

            Users? user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == username);
            if (user == null)
            {
                return new AuthResult(false, "User not found.");
            }

            user.password_hash = PasswordHasher.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return new AuthResult(true, string.Empty);
        }

        public async Task<AuthResult> UpdatePinAsync(string username, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return new AuthResult(false, "PIN cannot be empty.");
            }

            Users? user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == username);
            if (user == null)
            {
                return new AuthResult(false, "User not found.");
            }

            user.pin_hash = PasswordHasher.HashPassword(pin);
            await _context.SaveChangesAsync();
            return new AuthResult(true, string.Empty);
        }

        public Task LogoutAsync()
        {
            _settings.SetUser(null);
            return Task.CompletedTask;
        }
    }
}
