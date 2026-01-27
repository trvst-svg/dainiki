namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;

    public class JournalService : IJournalService
    {
        private readonly DainikiDbContext _context;

        public JournalService(DainikiDbContext context)
        {
            _context = context;
        }

        public async Task<JournalPageResult> GetJournalsAsync(string userId, JournalFilters filters, int page, int pageSize)
        {
            IQueryable<Journal> query = BuildQuery(userId, filters);

            int totalCount = await query.CountAsync();
            List<Journal> items = await query
                .OrderByDescending(journal => journal.journal_date)
                .ThenByDescending(journal => journal.journal_time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new JournalPageResult(items, totalCount);
        }

        public async Task<List<Journal>> GetJournalsInRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            JournalFilters filters = new JournalFilters(startDate, endDate, null, null, null, false);

            return await BuildQuery(userId, filters)
                .OrderBy(journal => journal.journal_date)
                .ThenBy(journal => journal.journal_time)
                .ToListAsync();
        }

        public async Task<Journal?> GetJournalAsync(int journalId, string userId)
        {
            return await _context.Journals
                .Include(journal => journal.category)
                .Include(journal => journal.JournalTags)
                    .ThenInclude(journalTag => journalTag.tag)
                .Include(journal => journal.JournalMoods)
                    .ThenInclude(journalMood => journalMood.mood)
                        .ThenInclude(mood => mood.category)
                .FirstOrDefaultAsync(journal => journal.journal_id == journalId && journal.user_id == userId);
        }

        public async Task<JournalSaveResult> SaveJournalAsync(JournalInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Title))
            {
                return new JournalSaveResult(false, 0, "Title is required.", false);
            }

            if (TextHelpers.IsContentEmpty(input.Content))
            {
                return new JournalSaveResult(false, 0, "Content is required.", false);
            }

            if (!input.PrimaryMoodId.HasValue)
            {
                return new JournalSaveResult(false, 0, "Primary mood is required.", false);
            }

            DateTime entryDate = input.EntryDate.Date;

            Journal? existingForDate = await _context.Journals
                .FirstOrDefaultAsync(journal =>
                    journal.user_id == input.UserId &&
                    journal.journal_date == entryDate);

            if (existingForDate != null)
            {
                if (!input.JournalId.HasValue || existingForDate.journal_id != input.JournalId.Value)
                {
                    return new JournalSaveResult(false, existingForDate.journal_id, "Only one entry per day is allowed.", false);
                }
            }

            Journal? journalFromDb = null;
            if (input.JournalId.HasValue)
            {
                journalFromDb = await _context.Journals.FirstOrDefaultAsync(candidate =>
                    candidate.journal_id == input.JournalId.Value &&
                    candidate.user_id == input.UserId);
            }

            bool isNew = journalFromDb == null;
            Journal journalEntry = journalFromDb ?? new Journal();

            if (isNew)
            {
                journalEntry.user_id = input.UserId;
                journalEntry.created_at = DateTime.Now;
                _context.Journals.Add(journalEntry);
            }

            journalEntry.journal_title = input.Title.Trim();
            journalEntry.journal_content_url = input.Content.Trim();
            journalEntry.journal_date = entryDate;
            journalEntry.journal_time = input.EntryDate.TimeOfDay;
            journalEntry.category_id = input.CategoryId;
            journalEntry.is_pinned = input.IsPinned;
            journalEntry.updated_at = DateTime.Now;

            string plainContent = TextHelpers.StripHtml(journalEntry.journal_content_url);
            journalEntry.word_count = TextHelpers.CountWords(plainContent);

            if (!isNew)
            {
                List<JournalTag> existingTags = await _context.JournalTags
                    .Where(journalTag => journalTag.journal_id == journalEntry.journal_id)
                    .ToListAsync();
                _context.JournalTags.RemoveRange(existingTags);

                List<JournalMood> existingMoods = await _context.JournalMoods
                    .Where(journalMood => journalMood.journal_id == journalEntry.journal_id)
                    .ToListAsync();
                _context.JournalMoods.RemoveRange(existingMoods);

                journalEntry.JournalTags = new List<JournalTag>();
                journalEntry.JournalMoods = new List<JournalMood>();
            }

            if (journalEntry.JournalTags == null)
            {
                journalEntry.JournalTags = new List<JournalTag>();
            }

            if (journalEntry.JournalMoods == null)
            {
                journalEntry.JournalMoods = new List<JournalMood>();
            }

            HashSet<int> addedTagIds = new HashSet<int>();
            foreach (int tagId in input.TagIds)
            {
                if (addedTagIds.Contains(tagId))
                {
                    continue;
                }

                journalEntry.JournalTags.Add(new JournalTag
                {
                    tag_id = tagId
                });
                addedTagIds.Add(tagId);
            }

            journalEntry.JournalMoods.Add(new JournalMood
            {
                mood_id = input.PrimaryMoodId.Value,
                mood_role = "Primary"
            });

            HashSet<int> addedMoodIds = new HashSet<int>();
            addedMoodIds.Add(input.PrimaryMoodId.Value);

            foreach (int secondaryMoodId in input.SecondaryMoodIds)
            {
                if (addedMoodIds.Contains(secondaryMoodId))
                {
                    continue;
                }

                journalEntry.JournalMoods.Add(new JournalMood
                {
                    mood_id = secondaryMoodId,
                    mood_role = "Secondary"
                });

                addedMoodIds.Add(secondaryMoodId);
            }

            await _context.SaveChangesAsync();
            return new JournalSaveResult(true, journalEntry.journal_id, string.Empty, isNew);
        }

        public async Task<bool> DeleteJournalAsync(int journalId, string userId)
        {
            Journal? journal = await _context.Journals
                .FirstOrDefaultAsync(entry => entry.journal_id == journalId && entry.user_id == userId);

            if (journal == null)
            {
                return false;
            }

            _context.Journals.Remove(journal);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Journal> BuildQuery(string userId, JournalFilters filters)
        {
            DateTime? startDate = filters.StartDate;
            DateTime? endDate = filters.EndDate;

            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                DateTime? temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            IQueryable<Journal> query = _context.Journals
                .Include(journal => journal.category)
                .Include(journal => journal.JournalTags)
                    .ThenInclude(journalTag => journalTag.tag)
                .Include(journal => journal.JournalMoods)
                    .ThenInclude(journalMood => journalMood.mood)
                        .ThenInclude(mood => mood.category)
                .Where(journal => journal.user_id == userId);

            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                query = query.Where(journal => journal.journal_date >= start);
            }

            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date;
                query = query.Where(journal => journal.journal_date <= end);
            }

            if (filters.PinnedOnly)
            {
                query = query.Where(journal => journal.is_pinned);
            }

            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                string term = "%" + filters.SearchTerm.Trim() + "%";
                query = query.Where(journal =>
                    EF.Functions.Like(journal.journal_title, term) ||
                    EF.Functions.Like(journal.journal_content_url, term));
            }

            if (filters.MoodId.HasValue)
            {
                int moodId = filters.MoodId.Value;
                query = query.Where(journal => journal.JournalMoods.Any(jm => jm.mood_id == moodId));
            }

            if (filters.TagId.HasValue)
            {
                int tagId = filters.TagId.Value;
                query = query.Where(journal => journal.JournalTags.Any(jt => jt.tag_id == tagId));
            }

            return query;
        }
    }
}
