namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Maui.Storage;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;

    public class PdfExportService : IPdfExportService
    {
        private readonly DainikiDbContext _context;
        private readonly IAnalyticsService _analyticsService;

        public PdfExportService(DainikiDbContext context, IAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        public async Task<string> Export(
            string userId,
            DateTime startDate,
            DateTime endDate,
            bool includeMood,
            bool includeTags,
            bool includeAnalytics)
        {
            includeMood = false;
            includeTags = false;
            includeAnalytics = false;

            DateTime rangeStart = startDate.Date;
            DateTime rangeEnd = endDate.Date;
            if (rangeEnd < rangeStart)
            {
                DateTime temp = rangeStart;
                rangeStart = rangeEnd;
                rangeEnd = temp;
            }

            List<Journal> journals = await _context.Journals
                .Include(journal => journal.JournalTags)
                    .ThenInclude(journalTag => journalTag.tag)
                .Include(journal => journal.JournalMoods)
                    .ThenInclude(journalMood => journalMood.mood!)
                        .ThenInclude(mood => mood.category)
                .Where(journal =>
                    journal.user_id == userId &&
                    journal.journal_date >= rangeStart &&
                    journal.journal_date <= rangeEnd)
                .OrderBy(journal => journal.journal_date)
                .ToListAsync();

            List<ExportEntry> entries = new List<ExportEntry>();
            foreach (Journal journal in journals)
            {
                string moodDisplay = BuildMoodDisplay(journal);
                string tagsDisplay = BuildTagsDisplay(journal);
                string contentPlain = TextHelpers.StripHtml(journal.journal_content_url);

                if (string.IsNullOrWhiteSpace(contentPlain))
                {
                    contentPlain = "(No content)";
                }

                ExportEntry entry = new ExportEntry
                {
                    Title = journal.journal_title,
                    DateDisplay = journal.journal_date.ToString("MMM dd, yyyy"),
                    MoodDisplay = moodDisplay,
                    TagsDisplay = tagsDisplay,
                    Content = contentPlain,
                    WordCount = journal.word_count
                };
                entries.Add(entry);
            }

            byte[] pdfBytes = BuildPdf(entries, null, rangeStart, rangeEnd, includeMood, includeTags, includeAnalytics);
            string fileName = "dainiki-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".pdf";
            string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
        }

        private static byte[] BuildPdf(
            List<ExportEntry> entries,
            DashboardMetrics? metrics,
            DateTime startDate,
            DateTime endDate,
            bool includeMood,
            bool includeTags,
            bool includeAnalytics)
        {
            string rangeText = startDate.ToString("MMM dd, yyyy") + " - " + endDate.ToString("MMM dd, yyyy");

            Document document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(style => style.FontSize(12));

                    page.Header().Text("Dainiki Export").FontSize(20).SemiBold();

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);
                        column.Item().Text("Range: " + rangeText).FontColor(Colors.Grey.Darken2);

                        AddEntries(column, entries, includeMood, includeTags);

                        if (includeAnalytics && metrics != null)
                        {
                            AddAnalytics(column, metrics);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated ").FontColor(Colors.Grey.Darken2);
                        text.Span(DateTime.Now.ToString("MMM dd, yyyy"));
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void AddEntries(ColumnDescriptor column, List<ExportEntry> entries, bool includeMood, bool includeTags)
        {
            if (entries.Count == 0)
            {
                column.Item().Text("No entries found for this range.");
                return;
            }

            foreach (ExportEntry entry in entries)
            {
                column.Item().Element(container =>
                {
                    container.Column(entryColumn =>
                    {
                        entryColumn.Spacing(4);
                        entryColumn.Item().Text(entry.Title).FontSize(14).SemiBold();
                        entryColumn.Item().Text(entry.DateDisplay).FontColor(Colors.Grey.Darken2);

                        if (includeMood)
                        {
                            entryColumn.Item().Text("Mood: " + entry.MoodDisplay);
                        }

                        if (includeTags)
                        {
                            entryColumn.Item().Text("Tags: " + entry.TagsDisplay);
                        }

                        entryColumn.Item().Text(entry.Content);
                        entryColumn.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });
                });
            }
        }

        private static void AddAnalytics(ColumnDescriptor column, DashboardMetrics metrics)
        {
            column.Item().PaddingTop(10).Text("Analytics Summary").FontSize(14).SemiBold();
            column.Item().Text("Total entries: " + metrics.TotalEntries);
            column.Item().Text("Current streak: " + metrics.CurrentStreak + " day(s)");
            column.Item().Text("Longest streak: " + metrics.LongestStreak + " day(s)");
            column.Item().Text("Missed days: " + metrics.MissedDays);

            if (metrics.HighestWordCountEntry != null)
            {
                column.Item().Text("Highest word count: " + metrics.HighestWordCountEntry.journal_title +
                                   " (" + metrics.HighestWordCountEntry.word_count + " words)");
            }

            if (metrics.MoodDistribution.Count > 0)
            {
                column.Item().PaddingTop(6).Text("Mood distribution").SemiBold();
                foreach (KeyValuePair<string, int> entry in metrics.MoodDistribution)
                {
                    column.Item().Text(entry.Key + ": " + entry.Value);
                }
            }

            if (metrics.TagBreakdown.Count > 0)
            {
                column.Item().PaddingTop(6).Text("Tag breakdown").SemiBold();
                foreach (KeyValuePair<string, int> entry in metrics.TagBreakdown)
                {
                    column.Item().Text(entry.Key + ": " + entry.Value);
                }
            }

            if (metrics.WordCountTrend.Count > 0)
            {
                column.Item().PaddingTop(6).Text("Word count trend").SemiBold();
                foreach (WordCountPoint point in metrics.WordCountTrend)
                {
                    column.Item().Text(point.Date.ToString("MMM dd, yyyy") + ": " + point.WordCount);
                }
            }
        }

        private static string BuildMoodDisplay(Journal journal)
        {
            string primary = "None";
            List<string> secondary = new List<string>();

            foreach (JournalMood journalMood in journal.JournalMoods)
            {
                if (journalMood.mood == null)
                {
                    continue;
                }

                if (journalMood.mood_role == "Primary")
                {
                    primary = journalMood.mood.mood_name;
                }
                else if (journalMood.mood_role == "Secondary")
                {
                    secondary.Add(journalMood.mood.mood_name);
                }
            }

            if (secondary.Count == 0)
            {
                return primary;
            }

            return primary + " (Secondary: " + string.Join(", ", secondary) + ")";
        }

        private static string BuildTagsDisplay(Journal journal)
        {
            List<string> tags = new List<string>();
            foreach (JournalTag journalTag in journal.JournalTags)
            {
                if (journalTag.tag != null)
                {
                    tags.Add(journalTag.tag.tag_name);
                }
            }

            if (tags.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", tags);
        }

        private class ExportEntry
        {
            public string Title { get; set; } = string.Empty;
            public string DateDisplay { get; set; } = string.Empty;
            public string MoodDisplay { get; set; } = string.Empty;
            public string TagsDisplay { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public int WordCount { get; set; }
        }
    }
}
