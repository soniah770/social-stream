using DataProcessor.Models;

namespace DataProcessor.Services
{
    public interface IPostProcessingService
    {// Process a single post
       Task<ProcessedPost> ProcessPostAsync(RawPost rawPost);
        // Process a batch of posts
        Task<List<ProcessedPost>> ProcessPostsBatchAsync(List<RawPost> rawPosts);
        // Additional  methods for processing
        bool IsSpamContent(string content);
        string AnalyzeSentiment(string content);
    }
}