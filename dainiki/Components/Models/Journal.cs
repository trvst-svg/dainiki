namespace dainiki.Components.Models;

public class Journal
{
    public int journal_id { get; set; }
    public string journal_title { get; set; } = string.Empty;
    public DateTime journal_date { get; set; }
    public TimeSpan journal_time { get; set; }
    public string journal_content_url { get; set; } = string.Empty;
    public int word_count { get; set; }
    public int category_id { get; set; }
    public string user_id { get; set; } = string.Empty;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public bool is_pinned { get; set; }

    public Category? category { get; set; }
    public Users? user { get; set; }
    
    public ICollection<JournalTag> JournalTags { get; set; } = new List<JournalTag>();
    public ICollection<JournalMood> JournalMoods { get; set; } = new List<JournalMood>();
}
