using Newtonsoft.Json;

namespace DataCollector.Security
{
    public class TokenResponse
    {
        // Make properties nullable or provide default values
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
        
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}