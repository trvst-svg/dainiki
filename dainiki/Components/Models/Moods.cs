namespace dainiki.Components.Models;

public class Mood
{
    public int mood_id { get; set; }
    public string mood_name { get; set; } = string.Empty;
    public int category_id { get; set; }
    public Category? category { get; set; }
    public ICollection<JournalMood> JournalMoods { get; set; } = new List<JournalMood>();
}
