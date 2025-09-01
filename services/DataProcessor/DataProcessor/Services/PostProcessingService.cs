using DataProcessor.Models;
using DataProcessor.Services;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;  // Add this line

namespace DataProcessor.Services
{
    public class PostProcessingService : IPostProcessingService
    {
        private readonly ILogger<PostProcessingService> _logger;
        
        // Performance: Static readonly collections for thread safety
        private static readonly string[] SpamKeywords = 
        {
            "buy now", "click here", "free money", "get rich", "limited time", 
            "act fast", "urgent", "winner", "congratulations"
        };
        
        private static readonly string[] PositiveWords = 
        {
            "great", "awesome", "love", "amazing", "excellent", "wonderful", 
            "fantastic", "brilliant", "perfect", "outstanding"
        };
        
        private static readonly string[] NegativeWords = 
        {
            "hate", "terrible", "awful", "worst", "disgusting", "horrible", 
            "disappointing", "frustrating", "annoying", "pathetic"
        };

        // Performance: Compiled regex for better performance
        private static readonly Regex HtmlTagRegex = new(@"<[^>]*>", RegexOptions.Compiled);
        private static readonly Regex HashtagRegex = new(@"#\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public PostProcessingService(ILogger<PostProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<List<ProcessedPost>> ProcessPostsBatchAsync(List<RawPost> rawPosts)
        {
            // Performance: Parallel processing with degree of parallelism control
            var options = new ParallelOptions 
            { 
                MaxDegreeOfParallelism = Environment.ProcessorCount 
            };

            var results = new ConcurrentBag<ProcessedPost>();
            
            await Parallel.ForEachAsync(rawPosts, options, async (rawPost, ct) =>
            {
                try
                {
                    var processed = await ProcessPostAsync(rawPost);
                    results.Add(processed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process post {Id}", rawPost.Id);
                }
            });

            return results.ToList();
        }

        public async Task<ProcessedPost> ProcessPostAsync(RawPost rawPost)
        {
            // Input validation for security
            if (string.IsNullOrWhiteSpace(rawPost?.Id))
            {
                throw new ArgumentException("Invalid raw post data", nameof(rawPost));
            }

            // Simulate processing delay without blocking threads
            await Task.Delay(5);

            var cleanContent = CleanContent(rawPost.Content ?? "");
            
            return new ProcessedPost
            {
                Id = rawPost.Id,
                Content = cleanContent,
                Author = SanitizeInput(rawPost.Author ?? "unknown"),
                Platform = rawPost.Platform?.ToLowerInvariant() ?? "unknown",
                Timestamp = rawPost.Timestamp,
                Hashtags = ExtractHashtags(cleanContent),
                LikeCount = Math.Max(0, rawPost.LikeCount),
                RetweetCount = Math.Max(0, rawPost.RetweetCount),
                SentimentScore = AnalyzeSentiment(cleanContent),
                IsSpam = IsSpamContent(cleanContent),
                ProcessedAt = DateTime.UtcNow
            };
        }

        public bool IsSpamContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            
            var lowerContent = content.ToLowerInvariant();
            return SpamKeywords.Any(keyword => lowerContent.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public string AnalyzeSentiment(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "neutral";
            
            var lowerContent = content.ToLowerInvariant();
            
            var positiveScore = PositiveWords.Count(word => 
                lowerContent.Contains(word, StringComparison.OrdinalIgnoreCase));
            var negativeScore = NegativeWords.Count(word => 
                lowerContent.Contains(word, StringComparison.OrdinalIgnoreCase));

            if (positiveScore > negativeScore) return "positive";
            if (negativeScore > positiveScore) return "negative";
            return "neutral";
        }

        private string CleanContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "";
            
            // Security: Remove HTML tags to prevent XSS
            var withoutHtml = HtmlTagRegex.Replace(content, " ");
            
            // Performance: Single pass normalization
            return withoutHtml
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("  ", " ")
                .Trim();
        }

        private List<string> ExtractHashtags(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new List<string>();
            
            return HashtagRegex.Matches(content)
                .Select(m => m.Value.ToLowerInvariant())
                .Distinct()
                .Take(10) // Security: Limit hashtags to prevent abuse
                .ToList();
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "unknown";
            
            // Security: Basic input sanitization
            return input.Trim()
                .Replace("<", "")
                .Replace(">", "")
                .Replace("script", "")
                .Substring(0, Math.Min(input.Length, 100)); // Length limit
        }
    }
}