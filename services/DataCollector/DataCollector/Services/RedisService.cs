using StackExchange.Redis;
using DataCollector.Services;

namespace DataCollector.Services
{
    public class RedisService : IRedisService  , IDisposable
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
        var subscribers = await _database.PublishAsync(RedisChannel.Literal(channel), message); //literal :Converts a string to a Redis channel name
        _logger.LogInformation("Successfully published to {Channel}, reached {Subscribers} subscribers", 
            channel, subscribers);
         // Checks if no subscribers received the message
        if (subscribers == 0)
        {
            _logger.LogWarning("Published to {Channel} but no subscribers received it", channel);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to publish to {Channel}", channel);
        throw;
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