namespace DataCollector.Models;

public class SocialPost
{
    public string Id
    { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public List<string> Hashtags { get; set; } = new();
    public int LikeCount { get; set; }
    public int RetweetCount { get; set; }
    public string OriginalUrl{get;set;}=string.Empty;
  public bool HasImage { get; set; }

}


