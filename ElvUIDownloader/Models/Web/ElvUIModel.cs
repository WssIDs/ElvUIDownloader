namespace ElvUIDownloader.Models.Web;

public class ElvUiModel
{
    public int id { get; set; }
    public string slug { get; set; }
    public string author { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string version { get; set; }
    public string changelog_url { get; set; }
    public string ticket_url { get; set; }
    public string git_url { get; set; }
    public List<string> patch { get; set; }
    public string last_update { get; set; }
    public string web_url { get; set; }
    public string donate_url { get; set; }
    public string small_desc { get; set; }
    public string screenshot_url { get; set; }
    public List<string> directories { get; set; }
}