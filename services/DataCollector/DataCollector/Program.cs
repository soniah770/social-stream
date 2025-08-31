using DataCollector;
using DataCollector.Services;
using DataCollector.Security;

var builder = Host.CreateApplicationBuilder(args);

// Register HTTP client with SocialMediaService
builder.Services.AddHttpClient<ISocialMediaService, SocialMediaService>();

// Register HTTP client with SocialMediaService
builder.Services.AddHttpClient<ISocialMediaService, SocialMediaService>();

// Register memory cache
builder.Services.AddMemoryCache();

// Register Redis service
builder.Services.AddSingleton<IRedisService, RedisService>();

// Register new security services
builder.Services.AddHttpClient<MastodonAuthenticationService>();
builder.Services.AddSingleton<MastodonAuthenticationService>();
builder.Services.AddSingleton<RateLimitService>();


// Register background worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
