namespace dainiki.Components.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardMetrics> GetMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
