//receiving processed posts from Redis and 
//Deserializes messages
//broadcasting them via SignalR to connected clients
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using ApiGateway.Hubs;
using ApiGateway.Models;
using Newtonsoft.Json;

namespace ApiGateway.Services
{
    public class PostStreamService : IPostStreamService, IDisposable
    {
        private readonly IHubContext<PostHub> _hubContext;
        private readonly ISubscriber _subscriber;
        private readonly ConnectionMultiplexer _connection;
        private readonly ILogger<PostStreamService> _logger;
        private bool _disposed = false;

        public PostStreamService(
            IHubContext<PostHub> hubContext,
            IConfiguration configuration,
            ILogger<PostStreamService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6380";
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 5000;
            
            _connection = ConnectionMultiplexer.Connect(options);
            _subscriber = _connection.GetSubscriber();
        }


        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            await _subscriber.SubscribeAsync(RedisChannel.Literal("processed-posts"),
                async (channel, message) =>
                {
                    try
                    {
                        var posts = JsonConvert.DeserializeObject<List<ProcessedPost>>(message);

                        if (posts?.Any() == true)
                        {
                            await _hubContext.Clients.Group("PostStream") // Allows sending messages to clients

                                .SendAsync("NewPosts", posts, cancellationToken); //"NewPosts" is the method name clients will receive

                            _logger.LogDebug("Broadcasted {Count} posts to PostStream group", posts.Count); //Helps track application activity
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error broadcasting posts to clients");
                    }
                });

            _logger.LogInformation("PostStreamService started listening to processed-posts");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken); //Resource management pause 

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