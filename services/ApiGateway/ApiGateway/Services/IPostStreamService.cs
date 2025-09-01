namespace ApiGateway.Services
{
    public interface IPostStreamService
    {// Starts listening to a stream of posts
        Task StartListeningAsync(CancellationToken cancellationToken);
    }
}