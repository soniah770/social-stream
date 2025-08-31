//for send messages 
namespace DataCollector.Services
{
    public interface IRedisService
    {

        Task PublishAsync(string channel, string messages); // Method to publish messages to a Redis channel
        Task<bool> IsConnectedAsync();       // Method to check Redis connection status
    }
}