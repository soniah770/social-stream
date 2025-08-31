using Microsoft.Extensions.Configuration;

namespace DataCollector.Security
{
    public class RateLimitService
    {
        private readonly SemaphoreSlim _rateLimitSemaphore;
        private readonly ILogger<RateLimitService> _logger;

        public RateLimitService(
            IConfiguration configuration, 
            ILogger<RateLimitService> logger)
        {
            var maxConcurrentRequests = configuration.GetValue("Mastodon:MaxConcurrentRequests", 5);
            _rateLimitSemaphore = new SemaphoreSlim(maxConcurrentRequests);
            _logger = logger;
        }

        public async Task<T> ExecuteWithRateLimitAsync<T>(Func<Task<T>> apiCall)
        {
            await _rateLimitSemaphore.WaitAsync();
            
            try
            {
                _logger.LogInformation("API call initiated");
                return await apiCall();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API call failed");
                throw;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
    }
}