namespace dainiki.Components.Services;

using dainiki.Components.Models;
using Microsoft.EntityFrameworkCore;

public class AuthService
{
    private readonly DainikiDbContext _context;
    private readonly AppState _appState;

    public AuthService(DainikiDbContext context, AppState appState)
    {
        _context = context;
        _appState = appState;
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(string username, string password)
    {
        var normalized = username.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username and password are required.");
        }

        var exists = await _context.Users.AnyAsync(user => user.username == normalized);
        if (exists)
        {
            return (false, "That username is already registered.");
        }

        var user = new Users
        {
            username = normalized,
            password_hash = PasswordHasher.HashPassword(password),
            auto_lock = true,
            hide_preview = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> LoginAsync(string username, string password)
    {
        var normalized = username.Trim();
        var user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == normalized);
        if (user == null)
        {
            return (false, "Invalid username or password.");
        }

        var matchesPassword = PasswordHasher.VerifyPassword(password, user.password_hash);
        var matchesPin = !string.IsNullOrWhiteSpace(user.pin_hash) &&
                         PasswordHasher.VerifyPassword(password, user.pin_hash);

        if (!matchesPassword && !matchesPin)
        {
            return (false, "Invalid username or password.");
        }

        _appState.SetUser(user);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdatePasswordAsync(string username, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return (false, "Password cannot be empty.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == username);
        if (user == null)
        {
            return (false, "User not found.");
        }

        user.password_hash = PasswordHasher.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdatePinAsync(string username, string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            return (false, "PIN cannot be empty.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(candidate => candidate.username == username);
        if (user == null)
        {
            return (false, "User not found.");
        }

        user.pin_hash = PasswordHasher.HashPassword(pin);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public Task LogoutAsync()
    {
        _appState.SetUser(null);
        return Task.CompletedTask;
    }
}
