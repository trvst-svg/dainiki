namespace dainiki.Components.Models;

public class Tag
{
    public int tag_id { get; set; }
    public string tag_name { get; set; }
    public bool system_tag { get; set; }
    public ICollection<JournalTag> JournalTags { get; set; }
}