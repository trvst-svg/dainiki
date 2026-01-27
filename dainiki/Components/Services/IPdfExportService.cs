namespace dainiki.Components.Services
{
    public interface IPdfExportService
    {
        Task<string> ExportAsync(
            string userId,
            DateTime startDate,
            DateTime endDate,
            bool includeMood,
            bool includeTags,
            bool includeAnalytics);
    }
}
