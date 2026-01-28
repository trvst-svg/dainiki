namespace dainiki.Components.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardMetrics> GetMetrics(string userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
