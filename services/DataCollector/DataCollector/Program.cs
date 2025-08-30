using DataCollector;
using DataCollector.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register HTTP client with SocialMediaService
builder.Services.AddHttpClient<ISocialMediaService, SocialMediaService>();

// Register memory cache
builder.Services.AddMemoryCache();

// Register Redis service
builder.Services.AddSingleton<IRedisService, RedisService>();

// Register background worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();