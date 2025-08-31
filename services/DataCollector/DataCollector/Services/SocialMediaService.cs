//for implementing interfaces services
using DataCollector.Models;
using DataCollector.Services;
using Microsoft.Extensions.Caching.Memory; 
using Newtonsoft.Json; 
using DataCollector.Security;  // New security namespace
using System.Net.Http.Headers;

public class SocialMediaService : ISocialMediaService
{
    // Existing dependencies
    private readonly HttpClient _httpClient; //for making API calls
    private readonly ILogger<SocialMediaService> _logger; //log message about whats happening
    private readonly IMemoryCache _cache; //store data temp in memory to avoid repeated calls

    // New security-related dependencies
    // Purpose: Centralize authentication and rate limiting
    private readonly MastodonAuthenticationService _authService;
    private readonly RateLimitService _rateLimitService;

    //constructor through dependency injection
    // Enhanced constructor to include security services
    public SocialMediaService(
        HttpClient httpClient, 
        ILogger<SocialMediaService> logger, 
        IMemoryCache cache,
        // Add new parameters for security services
        MastodonAuthenticationService authService,
        RateLimitService rateLimitService)
    {
        // Existing initialization
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;

        // New security service initialization
        // Dependency Injection: Receive authentication and rate limiting services
        _authService = authService;
        _rateLimitService = rateLimitService;

        // Security: Set timeout
        // Prevents long-running requests that could hang the system
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<List<SocialPost>> GetRecentPostsAsync(string platform)
    {
        // Enhanced method with rate limiting and authentication
        // Purpose: Add an extra layer of security and performance management
        return await _rateLimitService.ExecuteWithRateLimitAsync(async () => 
        {
            // Performance: Check cache first
            // Reduces unnecessary API calls
            var cacheKey = $"posts-{platform}";
            if (_cache.TryGetValue(cacheKey, out List<SocialPost>? cached) && cached != null)
            {
                return cached;
            }

            try
            {
                // Security: Automatic token retrieval
                // Ensures each request is authenticated
                var accessToken = await _authService.GetAccessTokenAsync();
                
                // Set authorization header for API request
                // Adds Bearer token to authenticate the request
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Fetch posts using existing logic
                var posts = await FetchMastodonPosts();

                // Performance: Cache results
                // Temporary storage to reduce subsequent API calls
                _cache.Set(cacheKey, posts, TimeSpan.FromSeconds(30));

                return posts;
            }
            catch (Exception ex)
            {
                // Comprehensive error logging
                // Ensures failures are tracked without breaking the application
                _logger.LogError(ex, "API call failed");
                return new List<SocialPost>();
            }
        });
    }

    //HTTP Request Method
    // Purpose: Fetch posts from Mastodon's public timeline
    private async Task<List<SocialPost>> FetchMastodonPosts()
    {
        // Make HTTP GET request to Mastodon's public timeline
        var response = await _httpClient.GetAsync("https://mastodon.social/api/v1/timelines/public?limit=5");
        
        // Ensure successful response
        response.EnsureSuccessStatusCode();

        // Convert response to string
        var json = await response.Content.ReadAsStringAsync();
        
        // Deserialize JSON to list of Mastodon API posts
        var mastodonPosts = JsonConvert.DeserializeObject<List<MastodonApiPost>>(json);

        // Convert to our standard SocialPost format
        // Use LINQ for efficient transformation
        return mastodonPosts?.Select(ConvertPost).ToList() ?? new List<SocialPost>();
    }

    // Convert Mastodon-specific post to our standard format
    // Purpose: Normalize data from different sources
    private SocialPost ConvertPost(MastodonApiPost post)
    {
        return new SocialPost
        {
            // Null-coalescing and null-conditional operators for safe data handling
            Id = post.Id ?? Guid.NewGuid().ToString(),
            Content = post.Content?.Replace("<p>", "").Replace("</p>", "") ?? "",
            Author = post.Account?.Username ?? "unknown",
            Platform = "mastodon",
            Timestamp = post.CreatedAt,
            LikeCount = post.FavouritesCount,
            RetweetCount = post.ReblogsCount,
            OriginalUrl = post.Url ?? string.Empty,
            HasImage = post.MediaAttachments != null && post.MediaAttachments.Any()
        };
    }
}