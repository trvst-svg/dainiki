namespace dainiki.Components.Services
{
    using dainiki.Components.Models;

    public interface IJournalService
    {
        Task<JournalPageResult> GetJournalsAsync(string userId, JournalFilters filters, int page, int pageSize);
        Task<List<Journal>> GetJournalsInRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Journal?> GetJournalAsync(int journalId, string userId);
        Task<JournalSaveResult> SaveJournalAsync(JournalInput input);
        Task<bool> DeleteJournalAsync(int journalId, string userId);
    }
}
