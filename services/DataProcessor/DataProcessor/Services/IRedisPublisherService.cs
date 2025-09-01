 // Publish processed posts to a Redis channel
using DataProcessor.Models;

namespace DataProcessor.Services
{
    public interface IRedisPublisherService
    {
        Task PublishProcessedPostsAsync(List<ProcessedPost> posts);
    }
}