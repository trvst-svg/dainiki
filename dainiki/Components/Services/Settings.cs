namespace dainiki.Components.Services
{
    using dainiki.Components.Models;

    public class Settings : ISettings
    {
        public Users? CurrentUser { get; private set; }

        public bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }

        public event Action? OnChange;

        public void SetUser(Users? user)
        {
            if (user != null)
            {
                var normalized = ThemeHelpers.NormalizeThemeName(user.theme);
                user.theme = string.IsNullOrWhiteSpace(normalized) ? null : normalized;
            }

            CurrentUser = user;
            NotifyStateChanged();
        }

        public void UpdateUserSettings(bool? hidePreview = null, bool? autoLock = null, string? theme = null)
        {
            if (CurrentUser == null)
            {
                return;
            }

            if (hidePreview.HasValue)
            {
                CurrentUser.hide_preview = hidePreview.Value;
            }

            if (autoLock.HasValue)
            {
                CurrentUser.auto_lock = autoLock.Value;
            }

            if (!string.IsNullOrWhiteSpace(theme))
            {
                CurrentUser.theme = ThemeHelpers.NormalizeThemeName(theme);
            }

            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            if (OnChange != null)
            {
                OnChange.Invoke();
            }
        }
    }
}
