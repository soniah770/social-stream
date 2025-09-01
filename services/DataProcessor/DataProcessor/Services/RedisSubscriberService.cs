using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataProcessor.Services
{
    public class RedisSubscriberService : IRedisSubscriberService, IDisposable
    {
        private readonly ISubscriber _subscriber;
        private readonly ConnectionMultiplexer _connection;
        private readonly ILogger<RedisSubscriberService> _logger;
        private bool _disposed = false;

        public event Func<string, Task>? OnMessageReceived;

        public RedisSubscriberService(
            IConfiguration configuration, 
            ILogger<RedisSubscriberService> logger)
        {
            _logger = logger;
            
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ReconnectRetryPolicy = new ExponentialRetry(1000);
            
            try 
            {
                _connection = ConnectionMultiplexer.Connect(options);
                _subscriber = _connection.GetSubscriber();
                _logger.LogInformation("Redis connected: {ConnectionString}", connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis connection failed: {ConnectionString}", connectionString);
                throw;
            }
        }

     public async Task StartListeningAsync(CancellationToken cancellationToken)
{
    try
    {
        await _subscriber.SubscribeAsync(
            RedisChannel.Literal("raw-posts"), // Use Literal method explicitly
            async (_, message) =>
            {
                if (cancellationToken.IsCancellationRequested) return;

                try 
                {
                    var messageText = message.ToString();
                    if (!string.IsNullOrEmpty(messageText) && OnMessageReceived != null)
                    {
                        await OnMessageReceived(messageText);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing error");
                }
            },
            CommandFlags.FireAndForget
        );

        // Keep connection alive
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogCritical(ex, "Subscription failed");
        throw;
    }
}

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}