namespace DataCollector;
using DataCollector.Models;
using DataCollector.Services;
using Newtonsoft.Json;

public class Worker : BackgroundService //Continuous background processing
{
    private readonly ILogger<Worker> _logger;
    private readonly ISocialMediaService _socialMediaService;
    private readonly IRedisService _redisService;
    private readonly IConfiguration _configuration; //Reads application configuration

    public Worker(ILogger<Worker> logger, ISocialMediaService socialMediaService,
            IRedisService redisService, IConfiguration configuration)
    {
        _logger = logger;
        _socialMediaService = socialMediaService;
        _redisService = redisService;
        _configuration = configuration;
    }

    //polymorphism - overriding base class method
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataCollector Worker started");
            // Runs continuously until service stops
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAndPublishPosts();

                var delayMinutes = _configuration.GetValue<int>("CollectionIntervalMinutes", 1);
                _logger.LogInformation("Waiting {DelayMinutes} minutes before next collection", delayMinutes);
                await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in collection cycle");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task CollectAndPublishPosts()
{
        // Fetch posts from social media service

    var posts = await _socialMediaService.GetRecentPostsAsync("mastodon");
    _logger.LogInformation("Collected {Count} posts from Mastodon", posts.Count);
    
    if (posts.Count > 0)
    {
        var jsonMessage = JsonConvert.SerializeObject(posts);
        _logger.LogInformation("JSON to publish (first 200 chars): {JsonPreview}", 
            jsonMessage.Substring(0, Math.Min(200, jsonMessage.Length)));
        
        await _redisService.PublishAsync("raw-posts", jsonMessage);
        _logger.LogInformation("Published {Count} posts to raw-posts channel", posts.Count);
    }
    else
    {
        _logger.LogWarning("No posts retrieved from social media service");
    }
}
}