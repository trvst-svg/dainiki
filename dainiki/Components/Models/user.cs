namespace dainiki.Components.Models;

public class Users
{
    public string username { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public string? pin_hash { get; set; }
    public bool hide_preview { get; set; }
    public bool auto_lock { get; set; }
    public string? theme { get; set; }
    public ICollection<Journal> Journals { get; set; } = new List<Journal>();
}
