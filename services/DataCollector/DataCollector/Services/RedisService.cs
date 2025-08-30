using StackExchange.Redis;
using DataCollector.Services;

namespace DataCollector.Services
{
    public class RedisService : IRedisServices  , IDisposable
    {
        private readonly IDatabase _database;
        private readonly ConnectionMultiplexer _connection;
        private readonly ILogger<RedisService> _logger;
        private bool _disposed = false;

        public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
        {
            _logger = logger;
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 5000; // Security: 5 second connect timeout
            options.SyncTimeout = 5000;    // Performance: 5 second operation timeout
            options.AbortOnConnectFail = false; // Resilience: Don't fail if Redis unavailable

            _connection = ConnectionMultiplexer.Connect(options);
            _database = _connection.GetDatabase();
            
            _logger.LogInformation("Redis connection established to {ConnectionString}", connectionString);
        }

        public async Task PublishAsync(string channel, string message)
        {
            try
            {
                // Security: Validate inputs
                if (string.IsNullOrWhiteSpace(channel) || string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("Invalid channel or message for publishing");
                    return;
                }

                // Performance: Use async publish
                var subscribers = await _database.PublishAsync(channel, message);
                _logger.LogDebug("Published to {Channel}, reached {Subscribers} subscribers", channel, subscribers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish to Redis channel {Channel}", channel);
                // Don't rethrow - let service continue running
            }
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                // Performance: Quick ping with timeout
                var pingResult = await _database.PingAsync();
                return pingResult.TotalMilliseconds < 1000; // Consider slow connections as disconnected
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
                _logger.LogInformation("Redis connection disposed");
            }
        }
    }
}