using Microsoft.Extensions.Configuration;
//Controls concurrent requests
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
            // read maximum concurrent requests from configuration
            var maxConcurrentRequests = configuration.GetValue("Mastodon:MaxConcurrentRequests", 5);
            _rateLimitSemaphore = new SemaphoreSlim(maxConcurrentRequests); //Controls how many simultaneous operations can occur
            _logger = logger;
        }
   //Generic Method
        public async Task<T> ExecuteWithRateLimitAsync<T>(Func<Task<T>> apiCall)
        {
            await _rateLimitSemaphore.WaitAsync();
            
            try
            {
                    // Log start of API call

                _logger.LogInformation("API call initiated");
                    // Execute the passed API call

                return await apiCall();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API call failed");
                throw;
            }
            finally
            {
                    // Always release the semaphore slot

                _rateLimitSemaphore.Release();
            }
        }
    }
}