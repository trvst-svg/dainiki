namespace dainiki.Components.Services
{
    public interface IPdfExportService
    {
        Task<string> Export(
            string userId,
            DateTime startDate,
            DateTime endDate,
            bool includeMood,
            bool includeTags,
            bool includeAnalytics);
    }
}
