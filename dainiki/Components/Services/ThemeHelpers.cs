namespace dainiki.Components.Services
{
    public static class ThemeHelpers
    {
        public static string NormalizeThemeName(string? theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
            {
                return string.Empty;
            }

            string normalized = theme.Trim();
            return normalized switch
            {
                "Daylight" => "Warm",
                "Nocturne" => "Dark",
                "Dusk" => "Pacific",
                _ => normalized
            };
        }
    }
}
