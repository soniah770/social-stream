namespace DataCollector;
using DataCollector.Models;
using DataCollector.Services;
using Newtonsoft.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
     private readonly ISocialMediaService _socialMediaService;
        private readonly IRedisService  _redisService;
        private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, ISocialMediaService socialMediaService,
            IRedisService redisService,
            IConfiguration configuration)
    {
        _logger = logger;
         _socialMediaService = socialMediaService;
            _redisService = redisService;
            _configuration = configuration;
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataCollector Worker started");

            if (!await _redisService.IsConnectedAsync())
            {
                _logger.LogError("Redis connection failed. Worker cannot start.");
                return;
            }


        while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectAndPublishPosts();
                    
                    var delayMinutes = _configuration.GetValue<int>("CollectionIntervalMinutes", 1);
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
            var posts = await _socialMediaService.GetRecentPostsAsync("mastodon");
            
            if (posts.Count > 0)
            {
                var jsonMessage = JsonConvert.SerializeObject(posts);
                await _redisService.PublishAsync("raw-posts", jsonMessage);
                
                _logger.LogInformation("Published {Count} posts to raw-posts channel", posts.Count);
            }
        }
    }
