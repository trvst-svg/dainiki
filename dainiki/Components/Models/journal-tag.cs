namespace dainiki.Components.Models;

public class JournalTag
{
    public int journal_id { get; set; }
    public int tag_id { get; set; }

    public Journal journal { get; set; }
    public Tag tag { get; set; }
}
