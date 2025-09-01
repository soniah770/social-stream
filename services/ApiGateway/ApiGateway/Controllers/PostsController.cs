using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ILogger<PostsController> _logger;
        private static readonly List<ProcessedPost> _recentPosts = new(); //Stores recent posts in memory
        private static readonly object _lock = new object(); //thread-safe operations:Prevents concurrent access to shared resource


        public PostsController(ILogger<PostsController> logger)
        {
            _logger = logger;
        }
        //Get Recent Posts Endpoint
        [HttpGet]
        public ActionResult<List<ProcessedPost>> GetRecentPosts([FromQuery] int limit = 20) //Flexible return type

        {
            lock (_lock)
            {
                var posts = _recentPosts
                    .OrderByDescending(p => p.Timestamp) //Sorts posts from newest to oldest
                    .Take(Math.Min(limit, 100)) //Limits number of posts returned(max=100)
                    .ToList();

                _logger.LogDebug("Returned {Count} recent posts", posts.Count);
                return Ok(posts);
            }
        }
   //Retrieve a specific post by its id
        [HttpGet("{id}")]
        public ActionResult<ProcessedPost> GetPost(string id)
        {
            lock (_lock)
            {
                var post = _recentPosts.FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    return NotFound($"Post with ID {id} not found");
                }
                return Ok(post);
            }
        }

        internal static void AddPost(ProcessedPost post)
        {
            lock (_lock)
            {
                _recentPosts.Insert(0, post);
                
                if (_recentPosts.Count > 1000)
                {
                    _recentPosts.RemoveRange(1000, _recentPosts.Count - 1000);
                }
            }
        }
    }
}