namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;

    public class AnalyticsService : IAnalyticsService
    {
        private readonly DainikiDbContext _context;

        public AnalyticsService(DainikiDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardMetrics> GetMetrics(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            {
                DateTime? temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            IQueryable<Journal> query = _context.Journals
                .Include(journal => journal.JournalMoods)
                    .ThenInclude(journalMood => journalMood.mood!)
                        .ThenInclude(mood => mood.category)
                .Include(journal => journal.JournalTags)
                    .ThenInclude(journalTag => journalTag.tag)
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

            List<Journal> journals = await query.ToListAsync();

            int totalEntries = journals.Count;
            Journal? highestWordCountEntry = null;
            int highestWordCount = -1;
            foreach (Journal journal in journals)
            {
                if (journal.word_count > highestWordCount)
                {
                    highestWordCount = journal.word_count;
                    highestWordCountEntry = journal;
                }
            }

            HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
            foreach (Journal journal in journals)
            {
                uniqueDates.Add(journal.journal_date.Date);
            }

            List<DateTime> entryDates = uniqueDates.ToList();
            entryDates.Sort();

            int longestStreak = CalculateLongestStreak(entryDates);
            int currentStreak = CalculateCurrentStreak(entryDates);
            int missedDays = CalculateMissedDays(entryDates, startDate, endDate);

            Dictionary<string, int> moodCounts = new Dictionary<string, int>();
            Dictionary<string, int> moodCategoryCounts = new Dictionary<string, int>();

            foreach (Journal journal in journals)
            {
                foreach (JournalMood journalMood in journal.JournalMoods)
                {
                    if (journalMood.mood_role != "Primary")
                    {
                        continue;
                    }

                    Mood? mood = journalMood.mood;
                    if (mood == null)
                    {
                        continue;
                    }

                    string moodName = mood.mood_name;
                    if (!moodCounts.ContainsKey(moodName))
                    {
                        moodCounts[moodName] = 0;
                    }
                    moodCounts[moodName]++;

                    string categoryName = "Uncategorized";
                    if (mood.category != null && !string.IsNullOrWhiteSpace(mood.category.category_name))
                    {
                        categoryName = mood.category.category_name;
                    }

                    if (!moodCategoryCounts.ContainsKey(categoryName))
                    {
                        moodCategoryCounts[categoryName] = 0;
                    }
                    moodCategoryCounts[categoryName]++;
                }
            }

            List<string> frequentMoods = BuildTopList(moodCounts, 3);
            IReadOnlyDictionary<string, int> moodDistribution = new Dictionary<string, int>(moodCategoryCounts);

            Dictionary<string, int> tagCounts = new Dictionary<string, int>();
            foreach (Journal journal in journals)
            {
                foreach (JournalTag journalTag in journal.JournalTags)
                {
                    if (journalTag.tag == null)
                    {
                        continue;
                    }

                    string tagName = journalTag.tag.tag_name;
                    if (!tagCounts.ContainsKey(tagName))
                    {
                        tagCounts[tagName] = 0;
                    }
                    tagCounts[tagName]++;
                }
            }

            List<string> frequentTags = BuildTopList(tagCounts, 5);
            IReadOnlyDictionary<string, int> tagBreakdown = new Dictionary<string, int>(tagCounts);

            Dictionary<DateTime, int> wordCountByDate = new Dictionary<DateTime, int>();
            foreach (Journal journal in journals)
            {
                DateTime date = journal.journal_date.Date;
                if (!wordCountByDate.ContainsKey(date))
                {
                    wordCountByDate[date] = 0;
                }
                wordCountByDate[date] += journal.word_count;
            }

            List<DateTime> wordCountDates = wordCountByDate.Keys.ToList();
            wordCountDates.Sort();

            List<WordCountPoint> wordCountTrend = new List<WordCountPoint>();
            foreach (DateTime date in wordCountDates)
            {
                wordCountTrend.Add(new WordCountPoint(date, wordCountByDate[date]));
            }

            return new DashboardMetrics(
                totalEntries,
                currentStreak,
                longestStreak,
                missedDays,
                highestWordCountEntry,
                frequentMoods,
                frequentTags,
                tagBreakdown,
                moodDistribution,
                wordCountTrend);
        }

        private static List<string> BuildTopList(Dictionary<string, int> counts, int take)
        {
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(counts);
            list.Sort((left, right) =>
            {
                int countCompare = right.Value.CompareTo(left.Value);
                if (countCompare != 0)
                {
                    return countCompare;
                }

                return string.Compare(left.Key, right.Key, StringComparison.Ordinal);
            });

            List<string> results = new List<string>();
            int limit = Math.Min(take, list.Count);
            for (int i = 0; i < limit; i++)
            {
                KeyValuePair<string, int> entry = list[i];
                results.Add(entry.Key + " (" + entry.Value + ")");
            }

            return results;
        }

        private static int CalculateLongestStreak(List<DateTime> dates)
        {
            if (dates.Count == 0)
            {
                return 0;
            }

            int longest = 1;
            int current = 1;

            for (int i = 1; i < dates.Count; i++)
            {
                if ((dates[i] - dates[i - 1]).Days == 1)
                {
                    current++;
                    if (current > longest)
                    {
                        longest = current;
                    }
                }
                else
                {
                    current = 1;
                }
            }

            return longest;
        }

        private static int CalculateCurrentStreak(List<DateTime> dates)
        {
            if (dates.Count == 0)
            {
                return 0;
            }

            HashSet<DateTime> dateSet = new HashSet<DateTime>(dates);
            int streak = 0;
            DateTime cursor = DateTime.Today;

            while (dateSet.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            return streak;
        }

        private static int CalculateMissedDays(List<DateTime> dates, DateTime? startDate, DateTime? endDate)
        {
            if (dates.Count == 0)
            {
                return 0;
            }

            DateTime start = (startDate ?? dates[0]).Date;
            DateTime end = (endDate ?? dates[dates.Count - 1]).Date;

            if (end < start)
            {
                DateTime temp = start;
                start = end;
                end = temp;
            }

            int totalDays = (end - start).Days + 1;
            HashSet<DateTime> dateSet = new HashSet<DateTime>(dates);
            int present = 0;

            for (DateTime day = start; day <= end; day = day.AddDays(1))
            {
                if (dateSet.Contains(day))
                {
                    present++;
                }
            }

            int missed = totalDays - present;
            if (missed < 0)
            {
                missed = 0;
            }

            return missed;
        }
    }

    public class WordCountPoint
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }

        public WordCountPoint()
        {
        }

        public WordCountPoint(DateTime date, int wordCount)
        {
            Date = date;
            WordCount = wordCount;
        }
    }

    public class DashboardMetrics
    {
        public int TotalEntries { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int MissedDays { get; set; }
        public Journal? HighestWordCountEntry { get; set; }
        public IReadOnlyList<string> FrequentMoods { get; set; }
        public IReadOnlyList<string> FrequentTags { get; set; }
        public IReadOnlyDictionary<string, int> TagBreakdown { get; set; }
        public IReadOnlyDictionary<string, int> MoodDistribution { get; set; }
        public IReadOnlyList<WordCountPoint> WordCountTrend { get; set; }

        public DashboardMetrics()
        {
            FrequentMoods = new List<string>();
            FrequentTags = new List<string>();
            TagBreakdown = new Dictionary<string, int>();
            MoodDistribution = new Dictionary<string, int>();
            WordCountTrend = new List<WordCountPoint>();
        }

        public DashboardMetrics(
            int totalEntries,
            int currentStreak,
            int longestStreak,
            int missedDays,
            Journal? highestWordCountEntry,
            IReadOnlyList<string> frequentMoods,
            IReadOnlyList<string> frequentTags,
            IReadOnlyDictionary<string, int> tagBreakdown,
            IReadOnlyDictionary<string, int> moodDistribution,
            IReadOnlyList<WordCountPoint> wordCountTrend)
        {
            TotalEntries = totalEntries;
            CurrentStreak = currentStreak;
            LongestStreak = longestStreak;
            MissedDays = missedDays;
            HighestWordCountEntry = highestWordCountEntry;
            FrequentMoods = frequentMoods;
            FrequentTags = frequentTags;
            TagBreakdown = tagBreakdown;
            MoodDistribution = moodDistribution;
            WordCountTrend = wordCountTrend;
        }
    }
}
