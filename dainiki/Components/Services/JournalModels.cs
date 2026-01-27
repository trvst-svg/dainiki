namespace dainiki.Components.Services
{
    using dainiki.Components.Models;

    public class JournalFilters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MoodId { get; set; }
        public int? TagId { get; set; }
        public string? SearchTerm { get; set; }
        public bool PinnedOnly { get; set; }

        public JournalFilters()
        {
        }

        public JournalFilters(
            DateTime? startDate,
            DateTime? endDate,
            int? moodId,
            int? tagId,
            string? searchTerm,
            bool pinnedOnly)
        {
            StartDate = startDate;
            EndDate = endDate;
            MoodId = moodId;
            TagId = tagId;
            SearchTerm = searchTerm;
            PinnedOnly = pinnedOnly;
        }
    }

    public class JournalPageResult
    {
        public List<Journal> Items { get; set; }
        public int TotalCount { get; set; }

        public JournalPageResult()
        {
            Items = new List<Journal>();
        }

        public JournalPageResult(List<Journal> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }

    public class JournalInput
    {
        public int? JournalId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime EntryDate { get; set; }
        public int CategoryId { get; set; }
        public bool IsPinned { get; set; }
        public int? PrimaryMoodId { get; set; }
        public IReadOnlyList<int> SecondaryMoodIds { get; set; }
        public IReadOnlyList<int> TagIds { get; set; }

        public JournalInput()
        {
            UserId = string.Empty;
            Title = string.Empty;
            Content = string.Empty;
            SecondaryMoodIds = new List<int>();
            TagIds = new List<int>();
        }

        public JournalInput(
            int? journalId,
            string userId,
            string title,
            string content,
            DateTime entryDate,
            int categoryId,
            bool isPinned,
            int? primaryMoodId,
            IReadOnlyList<int> secondaryMoodIds,
            IReadOnlyList<int> tagIds)
        {
            JournalId = journalId;
            UserId = userId;
            Title = title;
            Content = content;
            EntryDate = entryDate;
            CategoryId = categoryId;
            IsPinned = isPinned;
            PrimaryMoodId = primaryMoodId;
            SecondaryMoodIds = secondaryMoodIds;
            TagIds = tagIds;
        }
    }

    public class JournalSaveResult
    {
        public bool Success { get; set; }
        public int JournalId { get; set; }
        public string ErrorMessage { get; set; }
        public bool WasCreated { get; set; }

        public JournalSaveResult()
        {
            ErrorMessage = string.Empty;
        }

        public JournalSaveResult(bool success, int journalId, string errorMessage, bool wasCreated)
        {
            Success = success;
            JournalId = journalId;
            ErrorMessage = errorMessage;
            WasCreated = wasCreated;
        }
    }
}
