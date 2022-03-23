using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blog.Models
{
    public class Post
    {
        [JsonProperty("title")]
        public string? Title { get; set; }
        [JsonProperty("path")]
        public string? Path { get; set; }
        [JsonProperty("date")]
        public string? Date { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("isExternal")]
        public bool IsExternal { get; set; }
    }
}