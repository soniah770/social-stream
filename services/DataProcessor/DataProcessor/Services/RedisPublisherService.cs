using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using DataProcessor.Models;
using Newtonsoft.Json;

namespace DataProcessor.Services
{
    public class RedisPublisherService : IRedisPublisherService, IDisposable
    {
        private readonly IDatabase _database;
        private readonly ConnectionMultiplexer _connection;
        private readonly ILogger<RedisPublisherService> _logger;
        private bool _disposed = false;

       public RedisPublisherService(IConfiguration configuration, ILogger<RedisPublisherService> logger)
{
    _logger = logger;
      // Read Redis connection string
    var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
      

    var options = ConfigurationOptions.Parse(connectionString);
    options.ConnectTimeout = 5000;
    options.SyncTimeout = 5000;
    options.AbortOnConnectFail = false;  // Allow retries
    
    try 
    {
        _connection = ConnectionMultiplexer.Connect(options);
        _database = _connection.GetDatabase();
        _logger.LogInformation("Connected to Redis at {ConnectionString}", connectionString);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to connect to Redis at {ConnectionString}", connectionString);
        throw new InvalidOperationException($"Cannot connect to Redis. Make sure Redis is running at {connectionString}", ex);
    }
}
        public async Task PublishProcessedPostsAsync(List<ProcessedPost> posts)
        {
                // Early return if no posts

            if (!posts.Any()) return;

            try
            {         // Serialize posts to JSON

                var jsonMessage = JsonConvert.SerializeObject(posts, Formatting.None);
                var subscribers = await _database.PublishAsync(
                    RedisChannel.Literal("processed-posts"), jsonMessage);
                
                _logger.LogDebug("Published {Count} processed posts to {Subscribers} subscribers", 
                    posts.Count, subscribers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish {Count} processed posts", posts.Count);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}