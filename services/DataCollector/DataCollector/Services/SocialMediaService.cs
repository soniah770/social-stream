//for implementing interfaces services
using DataCollector.Models;

public class SocialMediaService : ISocialMediaService
{
    private readonly HttpClient _httpClient; //for making API calls
    private readonly ILogger<SocialMediaService> _logger; //log message about whats happening
    private readonly IMemoryCache _cache; //store data temp in memory to avoid repeated calls

//constructor through dependency injection
    public SocialMediaService(HttpClient httpClient, ILogger<SocialMediaService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;

        // Security: Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }


    public async Task<List<SocialPost>> GetRecentPostsAsync(string platform)
    {
        // Performance: Check cache first
        var cacheKey = $"posts-{platform}";
        if (_cache.TryGetValue(cacheKey, out List<SocialPost> cached))
        {
            return cached;
        }
        //for handling api calls

        try
        {
            var posts = await FetchMastodonPosts();

            // Performance: Cache for 30 seconds
            _cache.Set(cacheKey, posts, TimeSpan.FromSeconds(30));

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call failed");
            return new List<SocialPost>();
        }
    }
    //HTTP Request Method
    private async Task<List<SocialPost>> FetchMastodonPosts()
    {
        var response = await _httpClient.GetAsync("https://mastodon.social/api/v1/timelines/public?limit=5");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(); //convert response to string
        var mastodonPosts = JsonConvert.DeserializeObject<List<MastodonApiPost>>(json); //convert json to object

        return mastodonPosts?.Select(ConvertPost).ToList() ?? new List<SocialPost>(); //using linq to convert to list of social posts
    }

    private SocialPost ConvertPost(MastodonApiPost post)
    {
        return new SocialPost
        {
               Id = post.Id ?? Guid.NewGuid().ToString(),
                Content = post.Content?.Replace("<p>", "").Replace("</p>", "") ?? "",
                Author = post.Account?.Username ?? "unknown",
                Platform = "mastodon",
                TimeStamp = post.CreatedAt,
                LikeCount = post.FavouritesCount,
                RetweetCount = post.ReblogsCount,
                OriginalUrl = post.Url ?? string.Empty,
                HasImage = post.MediaAttachments != null && post.MediaAttachments.Any()
        };
    }
}