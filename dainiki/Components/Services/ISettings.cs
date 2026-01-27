namespace dainiki.Components.Services
{
    using dainiki.Components.Models;

    public interface ISettings
    {
        Users? CurrentUser { get; }
        bool IsAuthenticated { get; }
        event Action? OnChange;

        void SetUser(Users? user);
        void UpdateUserSettings(bool? hidePreview = null, bool? autoLock = null, string? theme = null);
    }
}
