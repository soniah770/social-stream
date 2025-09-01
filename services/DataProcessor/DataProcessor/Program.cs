using DataProcessor.Services;
using DataProcessor.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<IRedisSubscriberService, RedisSubscriberService>();
builder.Services.AddSingleton<IPostProcessingService, PostProcessingService>();
builder.Services.AddSingleton<IRedisPublisherService, RedisPublisherService>();
builder.Services.AddHostedService<ProcessorWorker>();

var host = builder.Build();
host.Run();

public class ProcessorWorker : BackgroundService
{
    private readonly IRedisSubscriberService _subscriber;
    private readonly IPostProcessingService _processor;
    private readonly IRedisPublisherService _publisher;
    private readonly ILogger<ProcessorWorker> _logger;

    public ProcessorWorker(
        IRedisSubscriberService subscriber,
        IPostProcessingService processor,
        IRedisPublisherService publisher,
        ILogger<ProcessorWorker> logger)
    {
        _subscriber = subscriber;
        _processor = processor;
        _publisher = publisher;
        _logger = logger;

        _subscriber.OnMessageReceived += ProcessMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataProcessor started listening for messages");
        await _subscriber.StartListeningAsync(stoppingToken);
    }

    private async Task ProcessMessage(string jsonMessage)
    {
        try
        {    // 1. Deserialize raw posts

            var rawPosts = JsonConvert.DeserializeObject<List<RawPost>>(jsonMessage);
            
            if (rawPosts == null || !rawPosts.Any())
            {
                _logger.LogWarning("Received empty or null message");
                return;
            }
        // 2. Process posts in batch

            var processedPosts = await _processor.ProcessPostsBatchAsync(rawPosts);
                // 3. Filter spam posts

            var validPosts = processedPosts.Where(p => !p.IsSpam).ToList();
            
            if (validPosts.Any())
            {
                await _publisher.PublishProcessedPostsAsync(validPosts);
                _logger.LogInformation("Published {ValidCount} clean posts, filtered {SpamCount} spam",
                    validPosts.Count, processedPosts.Count - validPosts.Count);
            }
            else
            {
                _logger.LogInformation("All {Count} posts were filtered as spam", processedPosts.Count);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON message format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }
}