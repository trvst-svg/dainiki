namespace dainiki.Components.Models;

public class JournalMood
{
    public int journal_id { get; set; }
    public int mood_id { get; set; }
    public string mood_role { get; set; }

    public Journal journal { get; set; }
    public Mood mood { get; set; }
}
