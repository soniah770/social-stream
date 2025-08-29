//for send messages 
namespace DataCollector.Services
{
    public interface IRedisServices
    {

        Task PublishAsync(string channel, string messages);
        Task<bool> IsConnectedAsync();
    }
}