//for send messages 
namespace DataCollector.Services
{
    public interface IRedisService
    {

        Task PublishAsync(string channel, string messages);
        Task<bool> IsConnectedAsync();
    }
}