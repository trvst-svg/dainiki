namespace dainiki.Components.Models;

public class Category
{
    public int category_id { get; set; }
    public string category_name { get; set; } = string.Empty;
    public string category_type { get; set; } = "Journal";
    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
    public ICollection<Mood> Moods { get; set; } = new List<Mood>();
}
