//for fetching post
using DataCollector.Models;
namespace DataCollector.Services;

public interface ISocialMediaService
{
Task<List<SocialPost>>GetRecentPostAsync(string platform);


}