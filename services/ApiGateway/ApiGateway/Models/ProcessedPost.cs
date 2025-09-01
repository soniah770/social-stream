namespace ApiGateway.Models
{
    public class ProcessedPost
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<string> Hashtags { get; set; } = new();
        public int LikeCount { get; set; }
        public int RetweetCount { get; set; }
        public string SentimentScore { get; set; } = "neutral";
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}