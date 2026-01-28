namespace dainiki.Components.Services
{
    using dainiki.Components.Models;

    public interface IJournalService
    {
        Task<JournalPageResult> GetJournals(string userId, JournalFilters filters, int page, int pageSize);
        Task<List<Journal>> GetJournalsInRange(string userId, DateTime startDate, DateTime endDate);
        Task<Journal?> GetJournal(int journalId, string userId);
        Task<JournalSaveResult> SaveJournal(JournalInput input);
        Task<bool> DeleteJournal(int journalId, string userId);
    }
}
