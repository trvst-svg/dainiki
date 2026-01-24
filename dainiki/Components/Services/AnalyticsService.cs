namespace dainiki.Components.Services;

using dainiki.Components.Models;
using Microsoft.EntityFrameworkCore;

public class AnalyticsService
{
    private readonly DainikiDbContext _context;

    public AnalyticsService(DainikiDbContext context)
    {
        _context = context;
    }

    public record WordCountPoint(DateTime Date, int WordCount);

    public record DashboardMetrics(
        int TotalEntries,
        int CurrentStreak,
        int LongestStreak,
        int MissedDays,
        Journal? HighestWordCountEntry,
        IReadOnlyList<string> FrequentMoods,
        IReadOnlyList<string> FrequentTags,
        IReadOnlyDictionary<string, int> TagBreakdown,
        IReadOnlyDictionary<string, int> MoodDistribution,
        IReadOnlyList<WordCountPoint> WordCountTrend);

    public async Task<DashboardMetrics> GetMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        var query = _context.Journals
            .Include(journal => journal.JournalMoods)
                .ThenInclude(journalMood => journalMood.mood)
                    .ThenInclude(mood => mood!.category)
            .Include(journal => journal.JournalTags)
                .ThenInclude(journalTag => journalTag.tag)
            .Where(journal => journal.user_id == userId);

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(journal => journal.journal_date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date;
            query = query.Where(journal => journal.journal_date <= end);
        }

        var journals = await query.ToListAsync();

        var totalEntries = journals.Count;
        var highestWordCountEntry = journals.OrderByDescending(journal => journal.word_count).FirstOrDefault();

        var entryDates = journals
            .Select(journal => journal.journal_date.Date)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        var longestStreak = CalculateLongestStreak(entryDates);
        var currentStreak = CalculateCurrentStreak(entryDates);
        var missedDays = CalculateMissedDays(entryDates, startDate, endDate);

        var primaryMoods = journals
            .SelectMany(journal => journal.JournalMoods
                .Where(journalMood => journalMood.mood_role == "Primary")
                .Select(journalMood => journalMood.mood))
            .Where(mood => mood != null)
            .Select(mood => mood!);

        var frequentMoods = primaryMoods
            .GroupBy(mood => mood.mood_name)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(3)
            .Select(group => $"{group.Key} ({group.Count()})")
            .ToList();

        var moodDistribution = primaryMoods
            .GroupBy(mood => mood.category?.category_name ?? "Uncategorized")
            .OrderByDescending(group => group.Count())
            .ToDictionary(group => group.Key, group => group.Count());

        var frequentTags = journals
            .SelectMany(journal => journal.JournalTags)
            .Where(journalTag => journalTag.tag != null)
            .Select(journalTag => journalTag.tag!.tag_name)
            .GroupBy(tag => tag)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(5)
            .Select(group => $"{group.Key} ({group.Count()})")
            .ToList();

        var tagBreakdown = journals
            .SelectMany(journal => journal.JournalTags)
            .Where(journalTag => journalTag.tag != null)
            .Select(journalTag => journalTag.tag!.tag_name)
            .GroupBy(tag => tag)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Count());

        var wordCountTrend = journals
            .GroupBy(journal => journal.journal_date.Date)
            .OrderBy(group => group.Key)
            .Select(group => new WordCountPoint(group.Key, group.Sum(journal => journal.word_count)))
            .ToList();

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

    private static int CalculateLongestStreak(List<DateTime> dates)
    {
        if (dates.Count == 0)
        {
            return 0;
        }

        var longest = 1;
        var current = 1;

        for (var i = 1; i < dates.Count; i++)
        {
            if ((dates[i] - dates[i - 1]).Days == 1)
            {
                current++;
                longest = Math.Max(longest, current);
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

        var dateSet = new HashSet<DateTime>(dates);
        var streak = 0;
        var cursor = DateTime.Today;

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

        var start = (startDate ?? dates.First()).Date;
        var end = (endDate ?? dates.Last()).Date;
        if (end < start)
        {
            (start, end) = (end, start);
        }

        var totalDays = (end - start).Days + 1;
        var dateSet = new HashSet<DateTime>(dates);
        var present = 0;

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (dateSet.Contains(day))
            {
                present++;
            }
        }

        return Math.Max(totalDays - present, 0);
    }
}
