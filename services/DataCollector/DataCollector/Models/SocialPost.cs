namespace DataCollector.Models;
using Newtonsoft.Json; // Allows JSON serialization/deserialization

public class SocialPost
{
    public string Id
    { get; set; } = string.Empty; //default value to avoid null issues

    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTime Timestamp  { get; set; }
    public List<string> Hashtags { get; set; } = new();
    public int LikeCount { get; set; }
    public int RetweetCount { get; set; }
    public string OriginalUrl{get;set;}=string.Empty;
  public bool HasImage { get; set; }

}


public class MastodonApiPost
    {
        public string? Id { get; set; }
        public string? Content { get; set; }
        //json property mapping
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        public string? Url { get; set; }
        [JsonProperty("favourites_count")]
        public int FavouritesCount { get; set; }
        [JsonProperty("reblogs_count")]
        public int ReblogsCount { get; set; }
        public MastodonAccount? Account { get; set; }
        [JsonProperty("media_attachments")]
        public List<MediaAttachment>? MediaAttachments { get; set; }
    }

    public class MastodonAccount
    {
        public string? Username { get; set; }
        [JsonProperty("display_name")]
        public string? DisplayName { get; set; }
    }

    public class MediaAttachment
    {
        public string? Id { get; set; } //migt be empty or have value
        public string? Type { get; set; }
    }
