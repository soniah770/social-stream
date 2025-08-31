//for implementing interfaces services
using DataCollector.Models;
using DataCollector.Services;
using Microsoft.Extensions.Caching.Memory; 
using Newtonsoft.Json; 
using DataCollector.Security;
using System.Net.Http.Headers;

public class SocialMediaService : ISocialMediaService
{
    // Dependencies
    private readonly HttpClient _httpClient;
    private readonly ILogger<SocialMediaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly MastodonAuthenticationService _authService;
    private readonly RateLimitService _rateLimitService;

    // Constructor with Dependency Injection
    public SocialMediaService(
        HttpClient httpClient, 
        ILogger<SocialMediaService> logger, 
        IMemoryCache cache,
        MastodonAuthenticationService authService,
        RateLimitService rateLimitService)
    {
        // Initialize dependencies
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
        _authService = authService;
        _rateLimitService = rateLimitService;

        // Set HTTP client timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<List<SocialPost>> GetRecentPostsAsync(string platform)
    {
        // Execute with rate limiting and authentication
        return await _rateLimitService.ExecuteWithRateLimitAsync(async () => 
        {
            // Check cache first
            var cacheKey = $"posts-{platform}";
            if (_cache.TryGetValue(cacheKey, out List<SocialPost>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                // Retrieve and set access token
                var accessToken = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Fetch and process posts
                var posts = await FetchMastodonPosts();

                // Cache results
                _cache.Set(cacheKey, posts, TimeSpan.FromSeconds(30));

                return posts;
            }
            catch (Exception ex)
            {
                // Log and handle errors
                _logger.LogError(ex, "API call failed");
                return new List<SocialPost>();
            }
        });
    }

    // Fetch posts from Mastodon's public timeline
    private async Task<List<SocialPost>> FetchMastodonPosts()
    {
        var response = await _httpClient.GetAsync("https://mastodon.social/api/v1/timelines/public?limit=5");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var mastodonPosts = JsonConvert.DeserializeObject<List<MastodonApiPost>>(json);

        // Convert to standard SocialPost format
        return mastodonPosts?.Select(ConvertPost).ToList() ?? new List<SocialPost>();
    }

    // Convert Mastodon-specific post to standard format
    private SocialPost ConvertPost(MastodonApiPost post)
    {
        return new SocialPost
        {
            Id = post.Id ?? Guid.NewGuid().ToString(),
            Content = post.Content?.Replace("<p>", "").Replace("</p>", "") ?? "",
            Author = post.Account?.Username ?? "unknown",
            Platform = "mastodon",
            Timestamp = post.CreatedAt,
            LikeCount = post.FavouritesCount,
            RetweetCount = post.ReblogsCount,
            OriginalUrl = post.Url ?? string.Empty,
            HasImage = post.MediaAttachments?.Any() ?? false
        };
    }
}