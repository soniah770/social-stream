namespace DataProcessor.Services
{
    public interface IRedisSubscriberService
    {
        Task StartListeningAsync(CancellationToken cancellationToken); // Start listening to a Redis channel

        event Func<string, Task>? OnMessageReceived; // Event to handle received messages,delegate method
        //Allows multiple subscribers to listen

    }
}