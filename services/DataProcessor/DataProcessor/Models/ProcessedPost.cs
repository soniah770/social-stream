namespace DataProcessor.Models 
{
    public class SocialPost 
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<string> Hashtags { get; set; } = new();
        public int LikeCount { get; set; }
        public int RetweetCount { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;

        // Processing-specific fields
        public string SentimentScore { get; set; } = "neutral";
        public bool IsSpam { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
    }
}