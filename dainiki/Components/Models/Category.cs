namespace dainiki.Components.Models;

public class Category
{
    public int category_id { get; set; }
    public string category_name { get; set; }
    public ICollection<Journal> Journals { get; set; }
    public ICollection<Mood> Moods { get; set; }
}
